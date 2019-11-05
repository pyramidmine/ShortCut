using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace QueryDeployer
{
	class Program
	{
		static string ServerIp { get; } = "172.18.99.2";
		static short ServerPort { get; } = 1521;
		static string DatabaseName { get; } = "xe";
		static string UserId { get; } = "SYSTEM";
		static string UserPassword { get; } = "Ekswnr59";

		static int Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine(Properties.Resources.Usage);
				return -1;
			}
			string connectionString = $"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={ServerIp})(PORT={ServerPort})))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={DatabaseName})));User ID={UserId};Password={UserPassword};Connection Timeout=30;Pooling=true;Statement Cache Size=1;";
			string queryPackage =
				@"CREATE OR REPLACE EDITIONABLE PACKAGE ""PKG_ORDER_LIST"" AS
					PROCEDURE INSERT_ORDER (
						in_ORDER_ID IN NUMBER,
						in_ORDER_DATE IN DATE,
						in_ORDER_PRICE IN NUMBER );
					PROCEDURE SELECT_ORDER (
						in_ORDER_DATE IN DATE );
				END ""PKG_ORDER_LIST"";";
			string queryPackageBody =
				@"CREATE OR REPLACE EDITIONABLE PACKAGE BODY ""PKG_ORDER_LIST"" AS
				/*
				** Procedure: INSERT_ORDER
				*/
				PROCEDURE INSERT_ORDER (
					in_ORDER_ID IN NUMBER,
					in_ORDER_DATE IN DATE,
					in_ORDER_PRICE IN NUMBER )
				IS
				BEGIN
					INSERT INTO ORDER_LIST (ORDER_ID, ORDER_DATE, ORDER_PRICE)
						VALUES(in_ORDER_ID, in_ORDER_DATE, in_ORDER_PRICE);
				END INSERT_ORDER;
				/*
				** Procedure: SELECT_ORDER
				*/
				PROCEDURE SELECT_ORDER (
					in_ORDER_DATE IN DATE )
				IS
				BEGIN
					RETURN;
				END SELECT_ORDER;
				END ""PKG_ORDER_LIST"";";

			try
			{
				using (OracleConnection conn = new OracleConnection(connectionString))
				{
					// Oracle PACKAGE command
					using (OracleCommand cmd = new OracleCommand(queryPackage, conn))
					{
						conn.Open();
						cmd.CommandType = CommandType.Text;
						cmd.ExecuteNonQuery();

						conn.Close();
					}

					// Oracle PACKAGE BODY command
					using (OracleCommand cmd = new OracleCommand(queryPackageBody, conn))
					{
						conn.Open();
						cmd.CommandType = CommandType.Text;
						cmd.ExecuteNonQuery();
						conn.Close();
					}
				}
			}
			catch (OracleException ex)
			{
				Console.WriteLine(ex.Message);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return 0;
		}
	}
}
