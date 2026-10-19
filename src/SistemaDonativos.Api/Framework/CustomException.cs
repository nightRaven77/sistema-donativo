using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace apiDonativos.Framework
{
    [Serializable]
    public class CustomException:Exception
    {
        public CustomException()
        {

        }
        public CustomException(string text):base(String.Format("{0}", text))
        {

        }
    }
}
