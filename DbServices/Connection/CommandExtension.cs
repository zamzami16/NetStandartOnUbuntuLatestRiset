using Npgsql;

namespace DbServices.Connection;

public static class CommandExtension
{
    public static void SetConnections(this NpgsqlCommand Command, IAxTransaction AxTransaction)
    {
        Command.Connection = AxTransaction.Connection as NpgsqlConnection;
        Command.Transaction = AxTransaction.Transaction as NpgsqlTransaction;

        if (Command.Connection.State == System.Data.ConnectionState.Closed)
        {
            Command.Connection.Open();
        }
    }
}
