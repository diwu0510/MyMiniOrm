using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MyMiniOrm.Expressions;
using MyMiniOrm.Reflections;

namespace MyMiniOrm
{
    public class ConditionResolver : ExpressionVisitor
    {
        // 查询参数
        private readonly List<KeyValuePair<string, object>> _parameters = new List<KeyValuePair<string, object>>();

        // 要关联的属性
        private readonly List<string> _joinProperties = new List<string>();

        // 查询语句
        private readonly Stack<string> _stringStack = new Stack<string>();

        // 主表信息
        private readonly MyEntity _master;

        // 参数前缀
        private readonly string _prefix = "@";

        // 临时全局变量-记录CallExpression中调用的方法名称
        private string _tempMethod;

        // 参数序号，用于生成SqlParameter的Name
        private int _parameterIndex;

        private bool isBinary = false;


        public ConditionResolver(MyEntity entity)
        {
            _master = entity;
        }

        public string GetCondition()
        {
            var condition = string.Concat(_stringStack.ToArray());
            _stringStack.Clear();
            return condition;
        }


        public string ResolveExpression(Expression node)
        {
            if (node.NodeType == ExpressionType.AndAlso ||
                node.NodeType == ExpressionType.OrElse)
            {
                // and
                var expression = (BinaryExpression) node;
                var right = ResolveExpression(expression.Right);
                var op = node.NodeType.ToSqlOperator();
                var left = ResolveExpression(expression.Left);

                return $"({left} {op} {right})";
            }
            else if (node.NodeType == ExpressionType.MemberAccess)
            {
                // 参数属性、变量属性等
                var expression = (MemberExpression) node;
                var rootType = expression.RootExpressionType(out var stack);
                if (rootType == ExpressionType.Parameter)
                {
                    return $"{ResolveStackToField(stack)}";
                }
                else
                {
                    var val = node.GetValue();
                    var parameterName = GetParameterName();
                    _parameters.Add(new KeyValuePair<string, object>(parameterName, val));
                    return parameterName;
                }
            }
            else if (node.NodeType == ExpressionType.Call)
            {
                // 方法调用
            }
            else if (node.NodeType == ExpressionType.Constant)
            {
                // 常量、本地变量
                var val = node.GetValue();
                var parameterName = GetParameterName();
                _parameters.Add(new KeyValuePair<string, object>(parameterName, val));
                return parameterName;
            }
            else if(node.NodeType == ExpressionType.Equal ||
                    node.NodeType == ExpressionType.NotEqual ||
                    node.NodeType == ExpressionType.GreaterThan ||
                    node.NodeType == ExpressionType.GreaterThanOrEqual ||
                    node.NodeType == ExpressionType.LessThan ||
                    node.NodeType == ExpressionType.LessThanOrEqual)
            {
                // 二元操作符，等于、不等于、大于、小于等
                var expression = (BinaryExpression) node;
                var right = expression.Right;
                var left = expression.Left;
                var op = expression.NodeType.ToSqlOperator();
                var value = right.GetValue();

                _stringStack.Push($"{left} {op} {right}");
            }

            return string.Empty;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.AndAlso ||
                node.NodeType == ExpressionType.OrElse)
            {
                var right = node.Right;
                var op = node.NodeType.ToSqlOperator();
                var left = node.Left;

                _stringStack.Push($"({ResolveExpression(left)} {node.NodeType.ToSqlOperator()} {ResolveExpression(right)})");
            }
            else if(node.NodeType == ExpressionType.Equal ||
                    node.NodeType == ExpressionType.NotEqual ||
                    node.NodeType == ExpressionType.GreaterThan ||
                    node.NodeType == ExpressionType.GreaterThanOrEqual ||
                    node.NodeType == ExpressionType.LessThan ||
                    node.NodeType == ExpressionType.LessThanOrEqual)
            {
                if (node.Left.NodeType == ExpressionType.MemberAccess)
                {
                    var root = ((MemberExpression) node.Left).RootExpressionType(out var stack);
                    if (root == ExpressionType.Parameter)
                    {
                        var parameterName = GetParameterName();
                        _stringStack.Push(parameterName);
                        _stringStack.Push(node.NodeType.ToSqlOperator());
                        ResolveStackToField(stack);
                    }
                }
            }
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var value = node.Value;
            if (value is bool b)
            {
                var val = b ? "1=1" : "1=0";
                _stringStack.Push(val);
            }
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Not)
            {
                var expression = node.Operand;
                if (expression.NodeType == ExpressionType.MemberAccess)
                {
                    var rootType = ((MemberExpression) expression).RootExpressionType(out var stack);
                    if (rootType == ExpressionType.Parameter)
                    {
                        _stringStack.Push("=0");
                        ResolveStackToField(stack);
                    }
                }
            }
            return node;
        }

        

        private string ResolveStackToField(Stack<string> parameterStack)
        {
            if (parameterStack.Count == 2)
            {
                // 调用了导航属性
                var propertyName = parameterStack.Pop();
                var propertyFieldName = parameterStack.Pop();

                _joinProperties.Add(propertyName);

                var prop = _master.Properties.Single(p => p.Name == propertyName);
                var propertyEntity = MyEntityContainer.Get(prop.PropertyInfo.PropertyType);
                var propertyProperty = propertyEntity.Properties.Single(p => p.Name == propertyFieldName);

                return $"[{propertyName}].[{propertyProperty.FieldName}]";
            }
            else if (parameterStack.Count == 1)
            {
                var propertyName = parameterStack.Pop();
                var propInfo = _master.Properties.Single(p => p.Name == propertyName);
                return $"[{_master.TableName}].[{propInfo.FieldName}]";
            }
            else
            {
                throw new ArgumentException("尚未支持大于2层属性调用。如 student.Clazz.School.Id>10，请使用类似 student.Clazz.SchoolId > 0 替代");
            }
        }

        private string GetParameterName()
        {
            return $"{_prefix}__p_{_parameterIndex++}";
        }
    }
}
