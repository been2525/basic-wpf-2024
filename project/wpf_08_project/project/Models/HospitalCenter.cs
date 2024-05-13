namespace project.Models
{
    internal class HospitalCenter
    {
        public int Id { get; set; }
        public string NM { get; set; }
        public string LC { get; set; }
        public string TELNO { get; set; }
        public string OPER_BGNG_TM { get; set; }
        public string OPER_END_TM { get; set; }
        public string CLNIC_SCOPE { get; set; }

        public static string INSERT_QUERY = @"INSERT INTO [dbo].[HospitalCenter]
                                                           ([NM]
                                                           ,[LC]
                                                           ,[TELNO]
                                                           ,[OPER_BGNG_TM]
                                                           ,[OPER_END_TM]
                                                           ,[CLNIC_SCOPE])
                                                     VALUES
                                                           (@NM
                                                           ,@LC
                                                           ,@TELNO
                                                           ,@OPER_BGNG_TM
                                                           ,@OPER_END_TM
                                                           ,@CLNIC_SCOPE)";
        public static string SELECT_QUERY = @"SELECT [Id]
                                                      ,[NM]
                                                      ,[LC]
                                                      ,[TELNO]
                                                      ,[OPER_BGNG_TM]
                                                      ,[OPER_END_TM]
                                                      ,[CLNIC_SCOPE]
                                                  FROM [dbo].[HospitalCenter]";
        public static string NM_QUERY = @"SELECT [NM]
                                            FROM [EMS].[dbo].[HospitalCenter]";
        public static readonly string CHECK_QUERY = @"SELECT COUNT(*) 
                                                          FROM HospitalCenter
                                                         WHERE Id = @Id";

        public static readonly string DELETE_QUERY = @"DELETE FROM [dbo].[HospitalCenter] WHERE Id = @Id";
    }
}
