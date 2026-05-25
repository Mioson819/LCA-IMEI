using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace LCA_Project.Utilities
{
    public  class UIControls
    {
       private static readonly object key = new object();
        private static UIControls _instance;
        public static UIControls Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (key)
                    {
                        if (_instance == null)
                        {
                            _instance = new UIControls();
                        }
                    }
                }
                return _instance;
            }
        }
    }
}
