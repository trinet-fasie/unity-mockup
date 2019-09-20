namespace TM
{
    public static class VCompare
    {
        public new static bool Equals(dynamic a, dynamic b)
        {
            dynamic resultA = a == null ? 0 : a;
            dynamic resultB = b == null ? 0 : b;

            if (resultA.GetType() != resultB.GetType())
            {
                DynamicCast.CastValue(a, b, ref resultA, ref resultB);
            }

            return resultA == resultB;
        }
        
        public static bool NotEquals(dynamic a, dynamic b)
        {
            dynamic resultA = a == null ? 0 : a;
            dynamic resultB = b == null ? 0 : b;

            if (resultA.GetType() != resultB.GetType())
            {
                DynamicCast.CastValue(a, b, ref resultA, ref resultB);
            }

            return resultA != resultB;
        }
        
        public static bool Less(dynamic a, dynamic b)
        {
            double resultA = DynamicCast.ConvertToDouble(a);
            double resultB = DynamicCast.ConvertToDouble(b);
            
            return resultA < resultB;
        }
        
        public static bool LessOrEquals(dynamic a, dynamic b)
        {
            double resultA = DynamicCast.ConvertToDouble(a);
            double resultB = DynamicCast.ConvertToDouble(b);

            return resultA <= resultB;
        }
        
        public static bool Greater(dynamic a, dynamic b)
        {
            double resultA = DynamicCast.ConvertToDouble(a);
            double resultB = DynamicCast.ConvertToDouble(b);

            return resultA > resultB;
        }
        
        public static bool GreaterOrEquals(dynamic a, dynamic b)
        {
            double resultA = DynamicCast.ConvertToDouble(a);
            double resultB = DynamicCast.ConvertToDouble(b);

            return resultA >= resultB;
        }
        
        public static bool And(dynamic a, dynamic b)
        {
            bool resultA = DynamicCast.ConvertToBoolean(a);
            bool resultB = DynamicCast.ConvertToBoolean(b);

            return resultA && resultB;
        }
        
        public static bool Or(dynamic a, dynamic b)
        {
            bool resultA = DynamicCast.ConvertToBoolean(a);
            bool resultB = DynamicCast.ConvertToBoolean(b);

            return resultA || resultB;
        }

        public static bool Not(dynamic a)
        {
            bool resultA = DynamicCast.ConvertToBoolean(a);

            return !resultA;
        }

        public static bool NotEmpty(dynamic a)
        {
            if (a == null)
            {
                return false;
            }
            
            if (a is bool && a == false)
            {
                return false;
            }

            if (a is string)
            {
                return !string.IsNullOrEmpty(a);
            }

            if (((object) a).IsNumericType())
            {
                return a != 0;
            }

            return true;
        }
    }
}