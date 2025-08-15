namespace ParseFranDictionary

open DapperDb

[<CLIMutable>]
type Paragraph =
    {
        ParagraphId :int64
        WordKeyId   :int64 
        Word        :string
        Index       :int
        ParagraphStr   :string
        ReplaceStr     :string
    }

[<CLIMutable>]
type WordKey =
    {
        WordId  :int64
        Word :string
    }
  
    
module DbModel =

    //paragraph--------------------------------------------------------------------------------------------------
    
    let parInsertStrSql (data :Paragraph) = $"INSERT INTO Paragraph ( WordKeyId,Word,IndexPar,ParagraphStr,ReplaceStr ) values ( \
                                                    {data.WordKeyId} ,\
                                                    \'{data.Word}\' ,\
                                                    {data.Index} ,\
                                                    \'{data.ParagraphStr}\',\
                                                    \'{data.ReplaceStr}\' )\
                                                    "

    let parUpdateStrSql (data :Paragraph) :string = $"UPDATE Paragraph SET
                                               WordKeyId = {data.WordKeyId} ,
                                               Word = \'{data.Word}\' ,
                                               IndexPar = {data.Index} ,
                                               ParagraphStr = \'{data.ParagraphStr}\' ,
                                               ReplaceStr = \'{data.ReplaceStr}\'
                                               WHERE 
                                               ParagraphId = {data.ParagraphId}    
                                               "
    let parDeleteStrSql (data :Paragraph) :string = $"DELETE FROM Paragraph 
                                                WHERE ParagraphId = {data.ParagraphId}
                                                "
    let parGetAllStrSql =
        "SELECT ParagraphId,WordKeyId,Word,IndexPar,ParagraphStr FROM Paragraph"
    
    let parGetIdStrSql (id :int64) :string =
        $"SELECT ParagraphId,WordKeyId,Word,IndexPar,ParagraphStr FROM Paragraph WHERE ParagraphId = {id} "
    
    let parGetFilterStrSql (filter :string) :string =
        $"SELECT ParagraphId,WordKeyId,Word,IndexPar,ParagraphStr FROM Paragraph WHERE  {filter} "
    
    let getAllPar () =
        let sql = parGetAllStrSql
        getAll<Paragraph> sql
    
    let getIdPar (id :int64) =
        let sql = parGetIdStrSql id
        getId<Paragraph> sql
    
    let getFilterPar (filter :string) =
        let sql = parGetFilterStrSql filter
        getFilter<Paragraph> sql 
    
    let insertPar (data :Paragraph) =
        let sql = parInsertStrSql(data)
        insert sql
    
    let updatePar (data :Paragraph) =
        let sql = parUpdateStrSql data
        update sql
    
    let deletePar (data :Paragraph)=
        let sql = parDeleteStrSql data
        delete sql

    //WordKey------------------------------------------------------------------------------
    
    let wordInsertStrSql (data :WordKey) :string = $"INSERT INTO WordKey ( Word ) values ( \
                                                    \'{data.Word}\' ) "

    let wordUpdateStrSql (data :WordKey) :string = $"UPDATE WordKey SET
                                              Word = \'{data.Word}\'
                                              WHERE WordId = {data.WordId}
                                              "

    let wordDeleteStrSql (data :WordKey) :string = $"DELETE FROM WordKey
                                              WHERE WordId = {data.WordId}
                                              "

    let wordGetAllStrSql =
        "SELECT WordId,Word FROM WordKey"
    
    let wordGetIdStrSql (id :int64) :string =
        $"SELECT WordId,Word FROM WordKey WHERE WordId = {id} "
    
    let wordGetFilterStrSql (filter :string) :string =
        $"SELECT WordId,Word FROM WordKey WHERE  {filter} "

    let getAllWord () =
        let sql = wordGetAllStrSql
        getAll<WordKey> sql
    
    let getIdWord (id :int64) =
        let sql = wordGetIdStrSql id
        getId<WordKey> sql
    
    let getFilterWord (filter :string) =
        let sql = wordGetFilterStrSql filter
        getFilter<WordKey> sql 
        
    let insertWord (data :WordKey) =
        let sql = wordInsertStrSql(data)
        insert sql
    
    let updateWord (data :WordKey) =
        let sql = wordUpdateStrSql data
        update sql
    
    let deleteWord (data :WordKey)=
        let sql = wordDeleteStrSql data
        delete sql