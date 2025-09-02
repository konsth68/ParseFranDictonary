namespace ParseFranDictionary

open System
open System.Text.RegularExpressions
open System.Collections.Generic
open DbModel

type DictPar =
    {
        Word :string
        Index :int
        Paragraph :string
        ReplacePartStr :string
        ReplaceWordStr :string
    }

type FullDictPar =
    {
        Word :string        
        
        ParagraphItems :DictPar seq                          
    }

type resultSaveWordKey =
    {
        Res :int option
        Word :string
    }

type resultSaveParagraph =
    {
        Res :int option
        Word :string
        Index :int
    }
    
module ParseDict =

    let accented     = [|"а́"; "я́"; "у́"; "ю́"; "о́"; "е́"; "э́"; "и́"; "ы́"; "А́"; "Я́"; "У́"; "Ю́"; "О́"; "Е́"; "Э́"; "И́"|]

    let no_accented  = [|"а"; "я"; "у"; "ю"; "о"; "е"; "э"; "и"; "ы"; "А"; "Я"; "У"; "Ю"; "О"; "Е"; "Э"; "И"|]

    let GetIndex (word :string) =
        let index =
            if word.Contains("IV") then
                4
            elif word.Contains("IX") then
                9
            elif word.Contains("VIII") then
                8
            elif word.Contains("VII") then
                7
            elif word.Contains("VI") then
                6
            elif word.Contains("III") then 
                3
            elif word.Contains("II") then
                2
            elif word.Contains("I") then
                1
            elif word.Contains("V") then
                5
            elif word.Contains("X") then
                10
            else
                0    
        index
    
    let ClearWorld (ina :string array) (oua :string array) (word :string) =
        let mutable wr = word
        for i in [0..16] do
            wr <- wr.Replace(ina[i],oua[i])
        wr <- wr.Replace("|","")  
        wr <- wr.Replace("I","")
        wr <- wr.Replace("V","")
        wr <- wr.Replace("X","")
        wr <- wr.ToLower()
        wr <- wr.Trim()
        wr   

    let PrintAllArticle (sq:string seq)   =
        for a in sq do
            printfn $"{a}"

    //<p(|\s+[^>]*)>(.*?)<\/p\s*>
    let pRegexp = Regex("<p(|\s+[^>]*)><strong>(.*?)<\/p\s*>",RegexOptions.Singleline)  

    let strongRegexp = Regex("<p><strong(|\s+[^>]*)>(.*?)<\/strong\s*>",RegexOptions.Singleline)

    let replaceRegex = Regex ("^(.*?)\|")
    
    let charStartRegexp = Regex("<p><strong>\\w\\w</strong></p>")
    
    let ClearParagraph (str:string) :string =
        let s1 = str.Replace("\n\r"," ")
        let s2 = s1.Replace("\r\n"," ")
        let s3 = s2.Replace("\n"," ")
        let s4 = s3.Replace("\r"," ")        
        s4

    let GetWord (str:string) :string option =
        let mt = strongRegexp.Match str
        if mt.Success = true then
            Some ( mt.Groups[2].Value)
        else
            None
    
    let clearLatNum (word:string) =
         let s1 = word.Replace("I","")
         let s2 = s1.Replace("V","")
         let s3 =   s2.Replace("X","")
         let s4 = s3.Trim()
         s4
            
    let getReplacePartStr (word :string) =        
        let mt = replaceRegex.Match word
        if mt.Success = true then
            mt.Groups[1].Value |> clearLatNum
        else
            word |> clearLatNum

    let getReplaceWordStr (word :string) =        
        word |> clearLatNum
    
    let GetParagraph (str:string) =
        let mtc = pRegexp.Matches str
        if mtc.Count > 0 then
            Some (
                 seq {
                    for m in mtc do
                        let s = ClearParagraph m.Value                    
                        yield s
                    }
                )
        else
            None

    let GetNoOptionWord (str:string option) :string =
        match str with
        | Some w -> w
        | None -> ""

    let CreateDictNode (word :string) (par :string) :DictPar =
        let vc :DictPar =    
            {
                Index = GetIndex word
                Word = word |> ClearWorld accented no_accented
                Paragraph = par
                ReplacePartStr = getReplacePartStr word
                ReplaceWordStr = getReplaceWordStr word
            }
        vc
    
    let FillDict (par :string seq) =
        seq {
            for p in par do
                let word = GetWord p |> GetNoOptionWord
                CreateDictNode word p
            }
    
    let CreateFullDictItem ( word :string ) (dictParSeq :DictPar seq) =
        let fdp :FullDictPar =
            {
                Word = word
                
                ParagraphItems = dictParSeq                                                  
            }
        fdp
    
    let filterNnParagraph (par :DictPar) =
        not (charStartRegexp.IsMatch par.Paragraph)
            
    let CreateFullDictSeq (par :DictPar seq)  :FullDictPar seq =
        par
        |> Seq.filter filterNnParagraph
        |> Seq.groupBy (fun x -> x.Word)
        |> Seq.map (fun x  -> CreateFullDictItem (fst x) (snd x) )                
               
    let PrintDictSeq (vcs :DictPar seq) =
        Seq.iter (fun (x :DictPar) -> printfn $"{x.Index};{x.Word};{x.Paragraph} ") vcs
    
    let PrintFullDictSeq (vcs :FullDictPar seq) =
        Seq.iter (fun (x :FullDictPar) -> printfn $"{x.Word}"
                                          for i in x.ParagraphItems do
                                              printfn $"{i.Index} \t {i.Word} \t {i.Paragraph}"
                                          ) vcs
        
    
    //word to db
    
    let createWordKey word =
        let wk :WordKey =
            {
                WordId = 0
                Word = word
            }
        wk

    let SaveWordKeyDb (word :string) =
        let wk = createWordKey word
        let r = insertWord wk
        let rr :resultSaveWordKey =
            {
                Res = r
                Word = word
            }
        rr
    
    let SaveWordKeysDb(fds :FullDictPar seq) =
        Seq.map (fun (x :FullDictPar) ->  (SaveWordKeyDb x.Word) ) fds
    
    // paragraph to db
    
            
    
    let ReplaceTilda (str :string) =
        let s = str.Replace("∼","&Tilde;")
        s
        
    let escapeSymbolInParagraph (par :string) =
        let s1 = par.Replace(":","&colon;")
        let s2 = s1.Replace("?","&quest;")
        let s3 = s2.Replace("∼","&Tilde;");
        let s4 = s3.Replace("|","")         
        s4
        
    let createParagraph (dp: DictPar) (wKey :int64) =
        let pr :Paragraph =
            {
                ParagraphId = 0
                WordKeyId = wKey
                Word = dp.Word
                Index = dp.Index
                ParagraphStr = dp.Paragraph |> escapeSymbolInParagraph
                ReplacePartStr = dp.ReplacePartStr
                ReplaceWordStr = dp.ReplaceWordStr
            }
        pr
    
    //let replaceTildaInParagraph (par :Paragraph)        
    
    
    let createParagraphWordSeq (par :FullDictPar) (wKey :int64) :Paragraph seq =
          seq {
               for pr in par.ParagraphItems ->
                    let pr = createParagraph pr wKey
                    pr
              }  
    
    let SaveParagraphDb (par  :Paragraph) =       
        let r = insertPar par
        let rr :resultSaveParagraph =
            {
                Res = r
                Word = par.Word
                Index = par.Index
            }
        rr
    
    let fillDict (dict :Dictionary<string,int64>) (word :string) (id :int64) =
        dict.Add(word,id)

    let convertWordKeyToDict (wks :WordKey seq) =
        let dict = new Dictionary<string,int64>()
        wks |> Seq.iter (fun (x :WordKey) -> fillDict dict x.Word x.WordId )        
        dict
        
    let saveParagraphToDatabase (fdp :FullDictPar seq)  =
        let wks = getAllWord()                 
        let sWks :WordKey seq = Option.get wks
        let dict = convertWordKeyToDict sWks
        seq {
            for p in fdp do
                let id = dict[p.Word]
                let pars = createParagraphWordSeq p id
                for pp in pars do
                    let r = SaveParagraphDb pp
                    yield r
            }
