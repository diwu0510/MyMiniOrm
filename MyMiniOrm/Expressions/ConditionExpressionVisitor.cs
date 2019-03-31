using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MyMiniOrm.Reflections;

namespace MyMiniOrm.Expressions
{
    public class ConditionExpressionVisitor : ExpressionVisitor
    {
        private readonly Queue<ConditionClause> _queue = new Queue<ConditionClause>();

        public Queue<ConditionClause> GetStack()
        {
            return _queue;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.AndAlso ||
                node.NodeType == ExpressionType.OrElse)
            {
                _queue.Enqueue(new ConditionClause() { Type = node.NodeType, Expression = node.Right });
            }
            else
            {
                _queue.Enqueue(new ConditionClause() { Type = null, Expression = node });
            }

            Visit(node.Left);
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            _queue.Enqueue(new ConditionClause() { Type = null, Expression = node });
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _queue.Enqueue(new ConditionClause() { Type = null, Expression = node });
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _queue.Enqueue(new ConditionClause() { Type = null, Expression = node });
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            _queue.Enqueue(new ConditionClause() { Type = null, Expression = node });
            return node;
        }
    }

    public class ConditionClause
    {
        public ExpressionType? Type { get; set; }

        public Expression Expression { get; set; }
    }

    public class ExpressionConverter
    {
        private readonly MyEntity _master;

        public List<KeyValuePair<string, object>> _parameters = new List<KeyValuePair<string, object>>();

        public List<string> JoinPropertyList = new List<string>();

        private readonly Stack<string> _stringStack = new Stack<string>();

        private readonly string _prefix;

        private int _parameterIndex;

        private string _tempMethod;

        public ExpressionConverter(MyEntity master, string prefix = "@")
        {
            _master = master;
            _prefix = prefix;
        }

        public void Resolve(Queue<ConditionClause> clauses)
        {
            for (int i = 0; i < clauses.Count; i++)
            {
                var expression = clauses.Dequeue();
                if (expression.Type != null)
                {
                    _stringStack.Push(")");
                    
                }
            }
        }

        public void ResolveMemberAccessExpression(MemberExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var rootType = expression.RootExpressionType(out var parameterStack);

            if (rootType == ExpressionType.Parameter)
            {
                if (parameterStack.Count == 2)
                {
                    // 调用了导航属性
                    var propertyName = parameterStack.Pop();
                    var propertyFieldName = parameterStack.Pop();

                    JoinPropertyList.Add(propertyName);

                    var prop = _master.Properties.Single(p => p.Name == propertyName);
                    var propertyEntity = MyEntityContainer.Get(prop.PropertyInfo.PropertyType);
                    var propertyProperty = propertyEntity.Properties.Single(p => p.Name == propertyFieldName);

                    _stringStack.Push($"[{propertyName}].[{propertyProperty.FieldName}]");
                }
                else if (parameterStack.Count == 1)
                {
                    var propertyName = parameterStack.Pop();
                    var propInfo = _master.Properties.Single(p => p.Name == propertyName);
                    _stringStack.Push($"[{_master.TableName}].[{propInfo.FieldName}]");
                }
                else
                {
                    throw new ArgumentException("尚未支持大于2层属性调用。如 student.Clazz.School.Id>10，请使用类似 student.Clazz.SchoolId > 0 替代");
                }
            }
            else
            {
                var obj = ResolveValue(expression.GetValue());
                var parameterName = $"{_prefix}__p_{_parameterIndex++}";
                _parameters.Add(new KeyValuePair<string, object>(parameterName, obj));
                _stringStack.Push($" {parameterName} ");
            }
        }

        public void ResolveConstantExpression(ConstantExpression node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            var parameterName = $"{_prefix}__p_{_parameterIndex++}";
            var value = ResolveValue(node.Value);
            if (value.ToString() == "1=1" || value.ToString() == "1=0")
            {
                _stringStack.Push($" {value} ");
            }
            else
            {
                _parameters.Add(new KeyValuePair<string, object>(parameterName, value));
                _stringStack.Push($" {parameterName} ");
            }
        }

        public void ResolveMethodCallExpression(MethodCallExpression node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            if (node.Object != null &&
                node.Object.NodeType == ExpressionType.MemberAccess &&
                ((MemberExpression)node.Object).RootExpressionType() == ExpressionType.Parameter)
            {
                string format;

                if (node.Method.Name == "StartsWith" ||
                    node.Method.Name == "Contains" ||
                    node.Method.Name == "EndsWith")
                {
                    format = "({0} LIKE {1})";
                }
                else
                {
                    throw new NotSupportedException($"不受支持的方法调用 {node.Method.Name}");
                }
                // 解析的时候需要在其他方法内根据方法名拼接字符串，
                // 所以在这里需要一个全局变量保存方法名
                _tempMethod = node.Method.Name;
                
                ResolveMemberAccessExpression((MemberExpression)node.Object);
                ResolveExpression(node.Arguments[0]);

                var right = _stringStack.Pop();
                var left = _stringStack.Pop();
                _stringStack.Push(string.Format(format, left, right));
            }
            else
            {
                var obj = ResolveValue(node.GetValue());

                var parameterName = $"{_prefix}__p_{_parameterIndex++}";
                _parameters.Add(new KeyValuePair<string, object>(parameterName, obj));
                _stringStack.Push($" {parameterName} ");
            }
        }

        public void ResolveUnaryExpression(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Not && node.Operand.NodeType == ExpressionType.MemberAccess)
            {
                var rootType = ((MemberExpression)node.Operand).RootExpressionType(out var parameterStack);

                if (rootType == ExpressionType.Parameter)
                {
                    if (parameterStack.Count == 2)
                    {
                        // 调用了导航属性
                        var propertyName = parameterStack.Pop();
                        var propertyFieldName = parameterStack.Pop();

                        JoinPropertyList.Add(propertyName);

                        var prop = _master.Properties.Single(p => p.Name == propertyName);
                        var propertyEntity = MyEntityContainer.Get(prop.PropertyInfo.PropertyType);
                        var propertyProperty = propertyEntity.Properties.Single(p => p.Name == propertyFieldName);

                        _stringStack.Push($"[{propertyName}].[{propertyProperty.FieldName}]=0");
                    }
                    else if (parameterStack.Count == 1)
                    {
                        var propertyName = parameterStack.Pop();
                        var propInfo = _master.Properties.Single(p => p.Name == propertyName);
                        _stringStack.Push($"[{_master.TableName}].[{propInfo.FieldName}]=0");
                    }
                    else
                    {
                        throw new ArgumentException("尚未支持大于2层属性调用。如 student.Clazz.School.Id>10，请使用类似 student.Clazz.SchoolId > 0 替代");
                    }
                }
                else
                {
                    var obj = ResolveValue(node.GetValue());
                    var parameterName = $"{_prefix}__p_{_parameterIndex++}";
                    _parameters.Add(new KeyValuePair<string, object>(parameterName, obj));
                    _stringStack.Push($" {parameterName} ");
                }
            }
        }

        private void ResolveExpression(Expression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.MemberAccess:
                    ResolveMemberAccessExpression((MemberExpression)node);
                    break;
                case ExpressionType.Constant:
                    ResolveConstantExpression((ConstantExpression)node);
                    break;
                case ExpressionType.Call:
                    ResolveMethodCallExpression((MethodCallExpression)node);
                    break;
                case ExpressionType.Not:
                    ResolveUnaryExpression((UnaryExpression)node);
                    break;
            }
        }

        private object ResolveValue(object obj)
        { 
            switch (_tempMethod)
            {
                case "Contains":
                    obj = $"%{obj}%";
                    break;
                case "StartsWith":
                    obj = $"{obj}%";
                    break;
                case "EndsWith":
                    obj = $"%{obj}";
                    break;
            }

            _tempMethod = "";
            return obj;
        }
    }
}
