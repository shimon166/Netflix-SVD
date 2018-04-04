using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Exercise1.Model
{
    public class User
    {
        public User(int userId, string sourceCategory)
        {
            UserId = userId;
            SourceCategory = sourceCategory;
            MatrixIndex = -1;
        }

        public int MatrixIndex { get;  set; }

        public Int32 UserId { get; set; }

        public string SourceCategory { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is User)
            {
                return ((User)obj).UserId == UserId && ((User)obj).SourceCategory == SourceCategory;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return UserId.GetHashCode()+ SourceCategory.GetHashCode();
        }
    }
}
