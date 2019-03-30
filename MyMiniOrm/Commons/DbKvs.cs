using System.Collections.Generic;

namespace MyMiniOrm.Commons
{
    public class DbKvs : List<KeyValuePair<string, object>>
    {
        public static DbKvs New()
        {
            return new DbKvs();
        }

        public DbKvs Add(string key, object value)
        {
            Add(new KeyValuePair<string, object>(key, value));
            return this;
        }
    }
}
