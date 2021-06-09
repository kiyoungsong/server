using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TOS_FCM
{
    public class PM_Config
    {
        public string RecipeName = "";

        public bool Analog_Select = false;
        public bool Digital_Select = false;

        public List<Digital_Ch_Config> Digital_CH_list
            = new List<Digital_Ch_Config>();

        public List<Analog_Ch_Config> Analog_CH_list
            = new List<Analog_Ch_Config>();
    }

    public class Digital_Ch_Config
    {
        public double SpecValue;
        public double SpecPercent;
        public bool ParameterCheck_Flag;

        public string Name;
        public string Description;
        public string Unit;
    }
    public class Analog_Ch_Config
    {
        //Data Collection
        public double Raw_Min;
        public double Raw_Max;
        public double Display_Min;
        public double Display_Max;

        //Parameter
        public bool ParameterCheck_Flag;
        public double AVG_Value;
        public double AVG_Percent;
        public double MIN_Value;
        public double MIN_Percent;
        public double MAX_Value;
        public double MAX_Percent;
        //public string Type;

        public string Name;
        public string Description;
        public string Unit;
    }
}
