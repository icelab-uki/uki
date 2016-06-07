using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace P_Tracker2
{
    public class UKI_FightingICE
    {
        UKI uki;

        public UKI_FightingICE(UKI u)
        {
            this.uki = u;
        }

        public String path_fightingICE = "";

        public void fightingICE_zeroCmd()
        {
            try
            {

                List<string> stringList = new List<string>() { };
                stringList.Add("Special: 0");
                stringList.Add("Atk: 0");
                stringList.Add("X: 0");
                stringList.Add("Y: 0");
                stringList.Add("Xdouble: 0");
                TheTool.writeFile(stringList, path_fightingICE, false);
            }
            catch { }
        }

        public void fightingICE_sendInput()
        {
            List<string> stringList = new List<string>() { };
            stringList.Add("Special:" + uki.key_Special);
            stringList.Add("Atk:" + uki.key_Atk);
            stringList.Add("X:" + uki.key_X);
            stringList.Add("Y:" + uki.key_Y);
            stringList.Add("Xdouble:" + uki.key_X_double);
            TheTool.writeFile(stringList, path_fightingICE, false);
        }

        public void setPath(string path)
        {
            try
            {
                List<string> stringList = new List<string>() { };
                stringList.Add(path);
                TheTool.writeFile(stringList, TheURL.url_config_FTG, false);
                path_fightingICE = path;
            }
            catch { TheSys.showError("Error Save Setting"); }
        }

    }
}
