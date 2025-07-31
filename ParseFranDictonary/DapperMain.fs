namespace ParseFranDictionary

open System
open System.Data.SQLite
open Dapper

module Util =
    
    let unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)
    
    let ConvertOptIntToDateTime (inVal:int64 option) =
        let res = 
            match inVal with 
            | Some x -> DateTime(unixStart.Ticks + int64 x,System.DateTimeKind.Utc)        
            | None -> DateTime.MinValue
        res

    let ConvertOptStringToString (inVal:string option) =
        let res = 
            match inVal with 
            | Some x -> x
            | None -> ""
        res

    let ConvertOptIntToInt (inVal:int64 option) =
        let res = 
            match inVal with 
            | Some x -> x
            | None -> 0
        res

    let ConvertOptFloatToFloat (inVal:float option) =
        let res = 
            match inVal with 
            | Some x -> x
            | None -> 0.0
        res

module DapperDb = 
    
    type OptionHandler<'T>() =
        inherit SqlMapper.TypeHandler<option<'T>>()

        override _.SetValue(param, value) = 
            let valueOrNull = 
                match value with
                | Some x -> box x
                | None -> null

            param.Value <- valueOrNull    

        override _.Parse value =
            if isNull value || value = box DBNull.Value 
            then None
            else Some (value :?> 'T) 
      
    let mutable ConnectionString = "Data Source=./FranDict.db ;Version=3;"
  
    let mutable LogFlag = false
     
    let SetupLogFlag flag =
        LogFlag <- flag
        ()
    
    let LogDapperDb str =
        if LogFlag then
            Log.Logger.Information(str)
        
    let InitDb str =
     
        ConnectionString <- $"Data Source={str} ;Version=3;"
        SqlMapper.AddTypeHandler(typeof<option<int>>, OptionHandler<int>())
        SqlMapper.AddTypeHandler(typeof<option<string>>, OptionHandler<string>())
        SqlMapper.AddTypeHandler(typeof<option<DateTime>>, OptionHandler<DateTime>())
        SqlMapper.AddTypeHandler(typeof<option<float>>, OptionHandler<float>())
        
        LogDapperDb($"Init: Set Connection string {ConnectionString}")
        
        ()        

    let inline notNull value = not (obj.ReferenceEquals(value, null))
    
    let inline isNull value = obj.ReferenceEquals(value, null)
     
    let  QueryOneDapper<'T> (connStr :string) (sql :string) :'T option =

        use conn= new SQLiteConnection(connStr)

        let res = conn.QuerySingle<'T>(sql)
        
        let r = 
            if notNull res then
                LogDapperDb($"Query with sql = \"{sql}\" success on {res} result ")
                Some res                
            else
                LogDapperDb($"Query with sql = \"{sql}\" None ")                
                None        
        r

    let  QueryManyDapper<'T> (connStr :string) (sql :string) : 'T seq option =

        use conn= new SQLiteConnection(connStr)

        let res = conn.Query<'T>(sql)
        
        let r = 
            if notNull res then
                LogDapperDb($"Query with sql = \"{sql}\" success on count {Seq.length res} result ")
                Some res
            else
                LogDapperDb($"Query with sql = \"{sql}\" None ")                
                None
        r

    let  ExecuteDapper (connStr :string) (sql :string) : int =

        use conn = new SQLiteConnection(connStr)

        let res = conn.Execute(sql)

        LogDapperDb($"Query with sql = \"{sql}\" success on result = {res} ")
                        
        res        

    
    let  ExecuteTransactDapper (connStr :string) (sql :string) : int option =

        use conn = new SQLiteConnection(connStr)

        conn.Open()
        
        use tran = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted)

        let r = 
            try
                let res = conn.Execute(sql,tran)                
                tran.Commit()
                LogDapperDb($"Query with sql = \"{sql}\" success on result = {res} ")
                Some res
            with
            | ex ->
                LogDapperDb($"Query with sql = \"{sql}\" Rollback and exception {ex.Message} ")                
                tran.Rollback()
                None
        
        r        

    let  ExecuteOutsideTransactDapper (connStr :string) (sql :string) (tran :SQLiteTransaction) : int option =

        use conn = new SQLiteConnection(connStr)

        conn.Open()
        
        let r = 
            try
                let res = conn.Execute(sql,tran)
                //tran.Commit()
                LogDapperDb($"Query with sql = \"{sql}\" success on result = {res} ")
                Some res
            with
            | ex ->
                LogDapperDb($"Query with sql = \"{sql}\" Rollback and exception {ex.Message} ")                
                tran.Rollback()
                None
        
        r        
    
    let ExecuteTransactManyDapper (connStr :string) (sqlList :string list)  =
        
        use conn = new SQLiteConnection(connStr)

        conn.Open()
        
        use tran = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted)
        
        let r =
            try
                let res = sqlList |> List.map (fun x -> conn.Execute( x, tran) ) 
                LogDapperDb($"Query with sql = \"{sqlList}\" success on result = {res} ")
                tran.Commit()
                Some res
            with
            | ex ->
                LogDapperDb($"Query with sql = \"{sqlList}\" Rollback and exception {ex.Message} ")                
                tran.Rollback()
                None
        r
    
    
    let QueryOne<'T> (sql :string) = QueryOneDapper<'T> ConnectionString sql

    let QueryMany<'T> (sql :string) = QueryManyDapper<'T> ConnectionString sql

    let Execute (sql :string) = ExecuteDapper ConnectionString sql

    let ExecuteOutsideTransact (sql :string) (tran :SQLiteTransaction) = ExecuteOutsideTransactDapper ConnectionString sql tran 
    
    let ExecuteTransact (sql :string) = ExecuteTransactDapper ConnectionString sql
    
    let ExecuteTransactMany (sqlList :string list) = ExecuteTransactManyDapper ConnectionString sqlList
    
    let getAll<'T> (sql :string) = QueryMany<'T> sql       
    
    let getId<'T> (sql :string) = QueryOne<'T> sql       
        
    let getFilter<'T> (sql :string) = QueryMany<'T> sql
        
    let insert (sql :string) = ExecuteTransact sql
    
    let update (sql :string) = ExecuteTransact sql
    
    let delete (sql :string) = ExecuteTransact sql
    
    
   
