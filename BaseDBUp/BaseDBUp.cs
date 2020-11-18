using DbUp;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;

namespace Transversal.Util.BaseDBUp
{
    public abstract class BaseDBUp : IBaseDBUp
    {
        protected string ConnString; 
        protected Assembly Assembly;
        protected string Path = "";
        private readonly bool ByPath = false;
        protected string Pattern; 
        protected bool IsRevert = false;
        protected bool IncludeData = false;
        protected bool IncludeDevelop = false;
        protected bool IncludeStoredProcedure = false;

        private readonly string EndDefault = "-bd.sql";
        private readonly string EndRevert = "-rv.sql";
        private readonly string EndData = "-data.sql";
        private readonly string EndDevelop = "-dev.sql";
        private readonly string EndStoredProcedure = "-sp.sql";

        private readonly DataBaseType dataBaseType;

        public enum DataBaseType
        {
            SqlServer,
            Postgresql
        };

        protected BaseDBUp(string con, 
                                            Assembly assembly,
                                            string pattern, 
                                            bool? isrevert, 
                                            bool? includedata, 
                                            bool? includedevelop, 
                                            bool? includestored,
                                            DataBaseType _dataBaseType)
        {
            ConnString = con;
            Assembly = assembly;
            Pattern = pattern;
            dataBaseType = _dataBaseType;
            SetPatternsValues(pattern, isrevert, includedata, includedevelop, includestored);

        }

        protected BaseDBUp(string con, 
                                            string path, 
                                            string pattern, 
                                            bool? isrevert, 
                                            bool? includedata, 
                                            bool? includedevelop,
                                            bool? includestored,
                                            DataBaseType _dataBaseType)
        {
            ConnString = con;
            Path = path;
            ByPath = true;
            dataBaseType = _dataBaseType;
            SetPatternsValues( pattern,isrevert,includedata,includedevelop,includestored);
        }

        
        /// <summary>
        /// Genera las estructuras de validacion de los nombres de los scripts
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="isrevert"></param>
        /// <param name="includedata"></param>
        /// <param name="includedevelop"></param>
        /// <param name="includestored"></param>
        private void SetPatternsValues(string pattern,
                                            bool? isrevert,
                                            bool? includedata,
                                            bool? includedevelop,
                                            bool? includestored)
        {
            Pattern = pattern;
            IsRevert = isrevert ?? IsRevert;
            IncludeData = includedata ?? IncludeData;
            IncludeDevelop = includedevelop ?? IncludeDevelop;
            IncludeStoredProcedure = includestored ?? IncludeStoredProcedure;
        }

        /// <summary>
        /// Generar Migracion
        /// </summary>
        /// <returns></returns>
        public ResultMigration GenerateMigration()
        {
            DbUp.Engine.DatabaseUpgradeResult result;

            try
            {
                result = ByPath ? MigrateScripsPath() : MigrateAssembly();
            }
            catch (TimeoutException ex)
            {
                return new ResultMigration()
                {
                    IsValid = false,
                    Result = string.Format("DbUp TimeOut: " + ex.Message)
                };
            }
            catch (SqlException ex)
            {
                return new ResultMigration()
                {
                    IsValid = false,
                    Result = string.Format("DbUp Sql: " + ex.Message)
                };
            }
            catch (Exception ex)
            {
                return new ResultMigration()
                {
                    IsValid = false,
                    Result = string.Format("DbUp Error: " + ex.Message)
                };
            }

            ResultMigration r = new ResultMigration();

            if (!result.Successful)
            {
                r.IsValid = false;
                r.Result = result.Error.Message;
            }
            else
            {
                r.IsValid = true;
                r.Result = "Ok";
            }

            return r;
        }


        protected DbUp.Engine.DatabaseUpgradeResult MigrateScripsPath()
        {
            switch(dataBaseType)
            {
                case DataBaseType.SqlServer: return MigrateSqlServerScripsPath();
                    
                case DataBaseType.Postgresql: return MigratePostgresScripsPath();

                default: throw new ArgumentException("Tipo de BD no identifcado");
            }
        }

        protected DbUp.Engine.DatabaseUpgradeResult MigrateAssembly()
        {
            switch (dataBaseType)
            {
                case DataBaseType.SqlServer: return MigrateSqlServerAssembly();

                case DataBaseType.Postgresql: return MigratePostgresScripsAssembly();

                default: throw new ArgumentException("Tipo de BD no identifcado");
            }
        }

        #region MIGRATION BY TYPE
        /// <summary>
        /// Genera migracion a partir de un path
        /// </summary>
        /// <returns>Resultado de la migracion</returns>
        private DbUp.Engine.DatabaseUpgradeResult MigrateSqlServerScripsPath()
        {
            EnsureDatabase.For.SqlDatabase(ConnString);

            var upgrader =
                DeployChanges.To
                    .SqlDatabase(ConnString)
                    .WithScriptsFromFileSystem(Path,(s) => !IsRevert ? FilterFuncScriptName(s) : FilterFuncScriptNameRevert(s))
                    .WithTransaction()
                    .LogToConsole()
                    .Build()
                    ;

            upgrader.GetScriptsToExecute();
            return upgrader.PerformUpgrade();
        }

        private DbUp.Engine.DatabaseUpgradeResult MigratePostgresScripsPath()
        {
            EnsureDatabase.For.PostgresqlDatabase(ConnString);

            var upgrader =
                DeployChanges.To
                    .PostgresqlDatabase(ConnString)
                    .WithScriptsFromFileSystem(Path, (s) => !IsRevert ? FilterFuncScriptName(s) : FilterFuncScriptNameRevert(s))
                    .WithTransaction()
                    .LogToConsole()
                    .Build()
                    ;

            upgrader.GetScriptsToExecute();
            return upgrader.PerformUpgrade();
        }

        /// <summary>
        /// Genera una migracion a partir de los scripts asociados al contexto de la solucion
        /// </summary>
        /// <returns></returns>
        private DbUp.Engine.DatabaseUpgradeResult MigrateSqlServerAssembly()
        {
            EnsureDatabase.For.SqlDatabase(ConnString);

            var upgrader =
                DeployChanges.To
                    .SqlDatabase(ConnString)
                    .WithScriptsEmbeddedInAssembly(Assembly, (s) => !IsRevert? FilterFuncScriptName(s) : FilterFuncScriptNameRevert(s))
                    .WithTransaction()
                    .LogToConsole()
                    .Build();

            return upgrader.PerformUpgrade();
        }

        private DbUp.Engine.DatabaseUpgradeResult MigratePostgresScripsAssembly()
        {
            EnsureDatabase.For.PostgresqlDatabase(ConnString);

            var upgrader =
                DeployChanges.To
                    .PostgresqlDatabase(ConnString)
                    .WithScriptsEmbeddedInAssembly(Assembly, (s) => !IsRevert ? FilterFuncScriptName(s) : FilterFuncScriptNameRevert(s))
                    .WithTransaction()
                    .LogToConsole()
                    .Build();

            return upgrader.PerformUpgrade();
        }
        #endregion
        
        #region VALIDATE FUNCTIONS
        /// <summary>
        /// Funcion que valida el formato de los scripts ingresados
        /// </summary>
        /// <param name="s">String a validar</param>
        /// <returns>true si cumple, false e.o.c</returns>
        public bool FilterFuncScriptName(string s) =>       ValidateFuncStartPattern(s)
                                                            &&
                                                            (ValidateFuncDataScript(s) || ValidateFuncDevelopmentScript(s) || ValidateFuncSPScript(s) || ValidateFuncDBScript(s)) 
                                                            &&
                                                            !ValidateFuncEndPattern(s, EndRevert);

        public bool FilterFuncScriptNameRevert(string s) => ValidateFuncStartPattern(s)
                                                            &&
                                                            ValidateFuncEndPattern(s, EndRevert);

        public bool ValidateFuncStartPattern(string s)  => (s.Replace("\\", "/")).Replace(String.Format("{0}/", (Path.Replace("\\", "/"))), "").StartsWith(Pattern);

        public bool ValidateFuncEndPattern(string s, string pattern)  => (s.Replace("\\", "/")).Replace(String.Format("{0}/", (Path.Replace("\\", "/"))), "").EndsWith(pattern);

        public bool ValidateFuncDataScript(string s) => ValidateFuncEndPattern(s, EndData) &&  IncludeData  ;

        public bool ValidateFuncSPScript(string s) => ValidateFuncEndPattern(s, EndStoredProcedure) && IncludeStoredProcedure  ;

        public bool ValidateFuncDevelopmentScript(string s) => ValidateFuncEndPattern(s, EndDevelop) && IncludeDevelop ;

        public bool ValidateFuncDBScript(string s) => ValidateFuncEndPattern(s, EndDefault) ;
        #endregion

    }
}

