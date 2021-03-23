using System;

namespace C64Lib
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RawAssemblyAttribute : Attribute
    {
        public RawAssemblyAttribute()
        {
        }
        public string Asm { get; set; }
        public string Resource { get; set; }
        public int Order { get; set; } = 255;
    }
}