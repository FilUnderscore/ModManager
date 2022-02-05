using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomModManager
{
    public class ModManagerTestCommand : ConsoleCmdAbstract
    {
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            Log.Exception(new NullReferenceException("Haha Exception go brr."));
        }

        public override string[] GetCommands()
        {
            return new string[] { "exceptionbeep" };
        }

        public override string GetDescription()
        {
            return "yes.";
        }
    }
}
