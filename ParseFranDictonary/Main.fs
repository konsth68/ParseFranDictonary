namespace ParseFranDictionary

open ParseDict
open System
open System.IO

module Main =

    let WriteStrSecToFile (file :string) (strSeq :string seq) =
        use wr = new StreamWriter(file,false,Text.Encoding.UTF8)
    
        for ss in strSeq do
            wr.WriteLine ss
            
    let WriteDictParToFile (file :string) (dictParSeq :DictPar seq) =
        use wr = new StreamWriter(file,false,Text.Encoding.UTF8)
    
        for dp in dictParSeq do
            wr.WriteLine $"{dp.Word};{dp.Index};{dp.Paragraph}"
        
        
        
    [<EntryPoint>]
    let main argv =

        printfn "__START__"
        
        Console.OutputEncoding = Text.Encoding.UTF8 |> ignore

        let desktop = Environment.GetFolderPath (Environment.SpecialFolder.Desktop)

        let srcFile = desktop + "\\b-ts.html"
        
        let franDictString = System.IO.File.ReadAllText srcFile
        
        DapperDb.InitDb "c:\\Users\\Konsth\\AppData\\Local\\FranDict\\FranDict.db"
        
        use wr = new StreamWriter((desktop + "\\ResultFranDictToFile.tx"),false,Text.Encoding.UTF8)   
        
        //let list = getArticle productHtml
        let sqOp = GetParagraph franDictString
        let smString = Option.get sqOp
        
        (*
        
        WriteStrToFile (desktop+"\\fr_t1.txt") smString
        
        let fd = FillDict smString
        
        WriteDictParToFile (desktop + "\\fr_dictpar.txt") fd
        
        *)
        
        
        //let s = ReplaceTilda "багет;1;<p><strong>I баге́т</strong> <em>m</em> baguette <em>f (de bois)</em>;лепно́й &lt;фигу́рный&gt; ∼ moulure <em>f</em> ~</p>"
        //printfn $"{s}"
        
        
        // for test tilde ------------------------------------------------------------------------
        let fds = smString
                            |> FillDict
                            |> CreateFullDictSeq
         
        let resWorkKey = SaveWordKeysDb fds
        
        for rw in resWorkKey do
            if( rw.Res.IsNone ) then
                printfn $"None -- {rw.Word}"
                wr.WriteLine($"None -- {rw.Word}")
            else
               printfn $"{rw.Res} -- {rw.Word}"
        
        
        let resPar = saveParagraphToDatabase fds
        
        for rp in resPar do
            if( rp.Res.IsNone ) then
                printfn $"None -- {rp.Word} -- {rp.Index}"
                wr.WriteLine($"None -- {rp.Word} -- {rp.Index}")
            else
                printfn $"{rp.Res} -- {rp.Word} -- {rp.Index}"
        
        
        
        
        printfn "__END__"
        
        0
    