using UnityEngine;

namespace Blocto.Sdk.Core.Model
{
    public class AndroidCallback : AndroidJavaProxy 
    {
    
        public AndroidCallback() : base("com.example.unitykotlin.IAlertCallback")
        {
        } 
    
        public void onPositive(string message)
        {
            Debug.Log(message);
        }

        public void onNegative(string message)
        {
            Debug.Log(message);
        }

        public AndroidCallback(string javaInterface)
            : base(javaInterface) { }

        public AndroidCallback(AndroidJavaClass javaInterface)
            : base(javaInterface) { }
    }
}