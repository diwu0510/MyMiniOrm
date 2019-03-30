using MyMiniOrm.Commons;
using MyMiniOrm.Queryable;
using MyMiniOrm.Reflections;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using MyMiniOrm.Expressions;
using MyMiniOrm.SqlBuilders;

namespace MyMiniOrm
{
    public class MyDb
    {
        private readonly string _connectionString;

        public MyDb(string connectionString)
        {
            _connectionString = connectionString;
        }

        public MyQueryable<T> Query<T>() where T : class , IEntity, new()
        {
            return new MyQueryable<T>(_connectionString);
        }

        public T Load<T>(int id) where T : class, IEntity, new()
        {
            return new MyQueryable<T>(_connectionString).Where(t => t.Id == id).SingleOrDefault();
        }

        public T Load<T>(Expression<Func<T, bool>> where = null, params Expression<Func<T, object>>[] orderBy) where T : class , new ()
        {
            var query = new MyQueryable<T>(_connectionString);
            if (where != null)
            {
                query.Where(where);
            }

            if (orderBy.Length > 0)
            {
                foreach (var ob in orderBy)
                {
                    query.OrderBy(ob);
                }
            }

            return query.SingleOrDefault();
        }

        public List<T> Fetch<T>(Expression<Func<T, bool>> where = null, params Expression<Func<T, object>>[] orderBy) where T : class, new()
        {
            var query = new MyQueryable<T>(_connectionString);
            if (where != null)
            {
                query.Where(where);
            }

            if (orderBy.Length > 0)
            {
                foreach (var ob in orderBy)
                {
                    query.OrderBy(ob);
                }
            }

            return query.ToList();
        }

        public List<T> Fetch<T>(int pageIndex,
            int pageSize,
            out int recordCount,
            Expression<Func<T, bool>> where = null,
            params Expression<Func<T, object>>[] orderBy) where T : class, new()
        {
            var query = new MyQueryable<T>(_connectionString);
            if (where != null)
            {
                query.Where(where);
            }

            if (orderBy.Length > 0)
            {
                foreach (var ob in orderBy)
                {
                    query.OrderBy(ob);
                }
            }

            return query.ToPageList(pageIndex, pageSize, out recordCount);
        }

        #region 创建
        public int Insert<T>(T entity) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var sb = new StringBuilder("INSERT INTO ");
            sb.Append($"[{entityInfo.TableName}] (");
            sb.Append(string.Join(",",
                entityInfo.Properties.Where(p => !p.InsertIgnore).Select(p => $"[{p.FieldName}]")));
            sb.Append(" VALUES (");
            sb.Append(string.Join(",",
                entityInfo.Properties.Where(p => !p.InsertIgnore).Select(p => $"@{p.Name}")));
            sb.Append(");SELECT @@IDENTITY;");

            var parameterList = entityInfo
                .Properties
                .Where(p => !p.InsertIgnore)
                .Select(p => new SqlParameter($"@{p.Name}", p.PropertyInfo.GetValue(entity)));

            var command = new SqlCommand(sb.ToString());
            command.Parameters.AddRange(parameterList.ToArray());

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                command.Connection = conn;
                var id = (int)command.ExecuteScalar();
                entity.Id = id;
                return id;
            }
        }

        public int Insert<T>(List<T> entityList) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var sb = new StringBuilder("INSERT INTO ");
            sb.Append($"[{entityInfo.TableName}] (");
            sb.Append(string.Join(",",
                entityInfo.Properties.Where(p => !p.InsertIgnore).Select(p => $"[{p.FieldName}]")));
            sb.Append(" VALUES (");
            sb.Append(string.Join(",",
                entityInfo.Properties.Where(p => !p.InsertIgnore).Select(p => $"@{p.Name}")));
            sb.Append(");SELECT @@IDENTITY;");

            var count = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var entity in entityList)
                        {
                            using (var command = new SqlCommand(sb.ToString(), conn))
                            {
                                command.Parameters.AddRange(entityInfo

                                    .Properties
                                    .Where(p => !p.InsertIgnore)
                                    .Select(p => new SqlParameter($"@{p.Name}", p.PropertyInfo.GetValue(entity)))
                                    .ToArray());
                                entity.Id = (int)command.ExecuteScalar();
                                count++;
                            }
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        count = 0;
                    }
                }
            }

            return count;
        }
        #endregion

        #region 更新
        public int Update<T>(T entity) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var sb = new StringBuilder("UPDATE ");
            sb.Append($"[{entityInfo.TableName}] SET ");
            sb.Append(string.Join(",",
                entityInfo.Properties.Where(p => !p.UpdateIgnore).Select(p => $"[{p.FieldName}]=@{p.Name}")));
            sb.Append($" WHERE [{entityInfo.KeyColumn}]=@Id");

            var parameterList = entityInfo
                .Properties
                .Where(p => !p.UpdateIgnore)
                .Select(p => new SqlParameter($"@{p.Name}", p.PropertyInfo.GetValue(entity)));

            var command = new SqlCommand(sb.ToString());
            command.Parameters.AddRange(parameterList.ToArray());
            command.Parameters.AddWithValue("@Id", entity.Id);

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                command.Connection = conn;
                return command.ExecuteNonQuery();
            }
        }

        public int Update<T>(List<T> entityList) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var sb = new StringBuilder("UPDATE ");
            sb.Append($"[{entityInfo.TableName}] SET ");
            sb.Append(string.Join(",",
                entityInfo.Properties.Where(p => !p.UpdateIgnore).Select(p => $"[{p.FieldName}]=@{p.Name}")));
            sb.Append($" WHERE [{entityInfo.KeyColumn}]=@Id");

            var count = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var entity in entityList)
                        {
                            using (var command = new SqlCommand(sb.ToString(), conn, trans))
                            {
                                command.Parameters.AddRange(entityInfo
                                    .Properties
                                    .Where(p => !p.UpdateIgnore)
                                    .Select(p => new SqlParameter($"@{p.Name}", p.PropertyInfo.GetValue(entity)))
                                    .ToArray());
                                command.Parameters.AddWithValue("@Id", entity.Id);
                                count += command.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                    }
                    catch(Exception ex)
                    {
                        trans.Rollback();
                        count = 0;
                    }
                }
            }

            return count;
        }

        public int Update<T>(int id, DbKvs kvs)
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var setProperties = kvs.Where(kv => kv.Key != "Id").Select(kv => kv.Key);
            var includeProperties = entityInfo.Properties.Where(p => setProperties.Contains(p.Name)).ToList();
            if (includeProperties.Count == 0)
            {
                return 0;
            }

            var sql =
                $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE Id=@Id";
            var parameters = kvs.ToSqlParameters();
            parameters.Add(new SqlParameter("@Id", id));
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return command.ExecuteNonQuery();
            }
        }

        public int Update<T>(DbKvs kvs, Expression<Func<T, bool>> expression = null)
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var setProperties = kvs.Where(kv => kv.Key != "Id").Select(kv => kv.Key);
            var includeProperties = entityInfo.Properties.Where(p => setProperties.Contains(p.Name)).ToList();
            if (includeProperties.Count == 0)
            {
                return 0;
            }

            string sql;
            List<SqlParameter> parameters;
            if (expression == null)
            {
                sql =
                    $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE Id=@Id";
                parameters = kvs.ToSqlParameters();
            }
            else
            {
                var whereExpressionVisitor = new WhereExpressionVisitor<T>();
                whereExpressionVisitor.Visit(expression);
                var where = whereExpressionVisitor.GetCondition();
                var whereParameters = whereExpressionVisitor.GetParameters().ToSqlParameters();
                parameters = kvs.ToSqlParameters();
                parameters.AddRange(whereParameters);

                where = string.IsNullOrWhiteSpace(where) ? "1=1" : where;

                sql =
                    $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE {where}";
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return command.ExecuteNonQuery();
            }
        }

        public int Update<T>(T entity, IEnumerable<string> includes) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var includeProperties = entityInfo.Properties.Where(p => includes.Contains(p.Name) && p.Name != "Id").ToList();
            if (includeProperties.Count == 0)
            {
                return 0;
            }

            var sql =
                $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE Id=@Id";
            var parameters = new List<SqlParameter> {new SqlParameter("@Id", entity.Id)};

            foreach (var property in includeProperties)
            {
                parameters.Add(new SqlParameter($"@{property.Name}", property.PropertyInfo.GetValue(entity)));
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return command.ExecuteNonQuery();
            }
        }

        public int UpdateIgnore<T>(T entity, IEnumerable<string> ignore) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var includeProperties = entityInfo.Properties.Where(p => !ignore.Contains(p.Name) && p.Name != "Id").ToList();
            if (includeProperties.Count == 0)
            {
                return 0;
            }

            var sql =
                $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE Id=@Id";
            var parameters = new List<SqlParameter> { new SqlParameter("@Id", entity.Id) };

            foreach (var property in includeProperties)
            {
                parameters.Add(new SqlParameter($"@{property.Name}", property.PropertyInfo.GetValue(entity)));
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return command.ExecuteNonQuery();
            }
        }
        #endregion

        #region 删除

        public int Delete<T>(int id) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var sql = $"DELETE [{entityInfo.TableName}] WHERE [{entityInfo.KeyColumn}]=@Id";
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@Id", id);
                return command.ExecuteNonQuery();
            }
        }

        public int Delete<T>(IEnumerable<int> idList) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var sql = $"EXEC('DELETE [{entityInfo.TableName}] WHERE [{entityInfo.KeyColumn}] in ('+@Ids+')')";
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@Ids", string.Join(",", idList));
                return command.ExecuteNonQuery();
            }
        }
        #endregion

        #region 数量

        public int GetCount<T>(Expression<Func<T, bool>> expression = null) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            if (expression == null)
            {
                var sql = $"SELECT COUNT(0) FROM [{entityInfo.TableName}]";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    return (int)command.ExecuteScalar();
                }
            }
            else
            {
                var whereExpressionVisitor = new WhereExpressionVisitor<T>();
                whereExpressionVisitor.Visit(expression);
                var condition = whereExpressionVisitor.GetCondition();
                var parameters = whereExpressionVisitor.GetParameters();

                condition = string.IsNullOrWhiteSpace(condition) ? "1=1" : condition;

                var sql = $"SELECT COUNT(0) FROM [{entityInfo.TableName}] WHERE [{condition}]";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    command.Parameters.AddRange(parameters.ToSqlParameters().ToArray());
                    return (int)command.ExecuteScalar();
                }
            }
        }
        #endregion
    }
}
