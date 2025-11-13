//test CQC for QualIQon - starting with getting accses of data stored in the ARC 

// #r "nuget: ARCtrl"
#r "nuget: ARCtrl.NET, 2.0.2"
#r "nuget: ARCtrl.QueryModel, 3.0.0-alpha.1"

open ARCtrl
open ARCtrl.NET
// open ARCtrl.QueryModel


// let arc = ARC()
// let arcPath = @"/home/paulinehans/Dokumente/TestARCForQualIQon"
// let loadingARC = ARC.load("/home/paulinehans/Dokumente/test")

// let writing = arc.Write("/home/paulinehans/Dokumente/test") //lädt kopie in gewünschtes Directory

let path = @"/home/paulinehans/Dokumente/TestARCForQualIQon"

let arc = ARC.load path
arc.MakeDataFilesAbsolute()
//Zugriff auf Assay/etc
let lastData = (arc.GetAssay("dilutionSeriesChlamy_ASSAY").Tables.[0]) 
printfn "%A"lastData    

let blub = arc.S.[0].Tables.[0]
printfn"%A"blub



let sampleNameColumn : string = "test"  
let getSample (fN) =
    arc.PreviousParametersOf(fN).[sampleNameColumn].ValueText 

let condition1 : string = "test2"

let getCondition1 (fN) =
    lastData
    |> Seq.filter (fun x -> getSample x = fN)
    |> Seq.map (fun x ->
        match arc.PreviousValuesOf(x).[condition1].TryValueText with
        | Some x -> x
        | None -> ""
    )
    |> Seq.head

let columnToFilter : string = "test1"
let getFilterColumn (fN) =
    lastData
    |> Seq.filter (fun x -> getSample x = fN)
    |> Seq.map (fun x ->
        match arc.PreviousValuesOf(x).[columnToFilter].TryValueText with
        | Some x -> x
        | None -> ""
    )
    |> Seq.head

 