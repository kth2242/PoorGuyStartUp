using UnityEngine;
using System;
using System.Collections;

namespace Anonym.Util
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
        AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class ConditionalHideAttribute : PropertyAttribute
    {
        //The name of the bool field that will be in control
        public string ConditionalSourceField = "";

        public bool HidenInspector = false;
        public bool Inverse = false;

  		public ConditionalHideAttribute(string conditionalSourceField)
		{
			this.ConditionalSourceField = conditionalSourceField;
			this.HidenInspector = false;
		}
	
		public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector)
		{
            if (conditionalSourceField.Contains("!"))
            {
                Inverse = true;
                this.ConditionalSourceField = conditionalSourceField.Split('!')[1];
            }
            else
			    this.ConditionalSourceField = conditionalSourceField;
			this.HidenInspector = hideInInspector;            
		}
    }
}