using System;
using System.Windows;
using System.Diagnostics;

namespace ideaSpaceApplication.Model
{
    class KeyboardHelper
    {
        public static Process touchKeyB;

        public static Process showKeyboard()
        {
            touchKeyB = new Process();
            touchKeyB.StartInfo.FileName = "C:\\Program Files\\Common Files\\Microsoft Shared\\ink\\TabTip.exe";
            try{
                touchKeyB.Start();
                //MessageBox.Show(touchKeyB.Id.ToString());
            }catch(Exception ex)
            {
                MessageBox.Show("showKeyboard: "+ex.Message);
            }

            return touchKeyB;
        }

        public static void hideKeyboard()
        {
            try{
                //MessageBox.Show("Killing "+KeyboardHelper.touchKeyB.Id.ToString());
                touchKeyB.Close();
            }catch(Exception ex)
            {
                MessageBox.Show("hideKeyboard: "+ex.Message);
            }
        }
    }
}
