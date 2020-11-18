using System;
using System.Collections.Generic;
using System.Text;

namespace Transversal.Util.BaseDBUp
{
    interface IBaseDBUp
    {
        ResultMigration GenerateMigration();

    }
}