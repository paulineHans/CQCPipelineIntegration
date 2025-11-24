
//test CQC for QualIQon - starting with getting accses of data stored in the ARC 

#r "nuget: ARCtrl.NET, 2.0.2"
#r "nuget: ARCtrl.QueryModel, 3.0.0-alpha.1"

open System
open ARCtrl.FileSystem
open ARCtrl
open ARCtrl.NET
open ARCtrl.QueryModel
open System.IO 
open System

let path = @"/home/paulinehans/Dokumente/TestARCForQualIQon"
let path2= "/home/paulinehans/Dokumente/Arcs/CurtABC_ChlRe_PPI"

let arc = ARC.load path2
arc.MakeDataFilesAbsolute()
arc.DataContextMapping()

let getIdentifier = arc.AssayIdentifiers
printfn "%A" getIdentifier

//ASSAY CHECK

//check isa file 
//check for MeasurementType Proteomics/proteomics 
let tables (arc: ARC) =
        arc.Assays.Count > 0
        && arc.Assays
        |> Seq.exists (fun assay -> assay.Tables.Count > 0 )
tables arc

let measurementType = arc.Assays |> Seq.exists (fun x -> x.MeasurementType.Value.NameText.ToLower().Contains"proteomics")  
measurementType
let technologyType = arc.Assays |> Seq.exists (fun x -> x.MeasurementType.Value.NameText.ToLower().Contains"mass spectrometry")
technologyType

//check Assay tables 
//output files 

let outputFilesWiff = arc.GetAssay("dilutionSeriesChlamy_ASSAY").LastData
let verify : bool =
    outputFilesWiff
    |> List.exists (fun x -> x.Name.Contains("wiff"))
verify

//check for Digestion 
//Parameter Digestion 
let getTableIfExists (arc: ARC) : ArcTable option =
    if arc.Assays.Count > 0 && arc.Assays.[0].Tables.Count > 0
    then Some arc.Assays.[0].Tables.[0]
    else None
let table = getTableIfExists arc
table

//checkForTrypsin 
let trypsinTerm (header: CompositeHeader) =
    match header with 
    | CompositeHeader.Parameter oa -> Some oa 
    | _ -> None

let getOntologyListByHeaderOntology (table : ArcTable) (ontologyName : string) =
            let isOntologyHeader (header : CompositeHeader)=
                    match trypsinTerm header with
                    | Some oa -> oa.NameText = ontologyName
                    | None -> false     
            let colOption = ArcTable.tryGetColumnByHeaderBy isOntologyHeader table
            match colOption with
            | Some col ->  
                    col.Cells
                    |> Seq.map (fun (cell:CompositeCell) -> cell.AsTerm)
                    |> Seq.distinct
                    |> List.ofSeq
            | None -> [] 

let exe = getOntologyListByHeaderOntology table.Value "Digestion"
let validateTrypsin = exe |> List.exists (fun x -> x.Name.Value.Contains("Trypsin"))
validateTrypsin
let validateLysC = exe |> List.exists (fun x -> x.Name.Value.Contains("Lys-C"))
validateLysC

//check for anything with labeling 
let searchForLabeling = exe |> List.exists (fun x -> x.Name.Value.Contains("labeling"))
searchForLabeling

let exe1 = getOntologyListByHeaderOntology table.Value "Isotope labeling"
let validate15N = exe1 |> List.exists (fun x -> x.Name.Value.Contains("15N"))
validate15N




// CHECK RUNS 
let runs = arc.Runs.[0].Tables.[0]
let checkForMzMLFiles = runs.OutputNames |> List.exists(fun x -> x.Contains("mzML"))
checkForMzMLFiles

let checkForMzLightFiles = 
    let data = arc.Runs.[0].Tables.[1]
    let validation = data.OutputNames |> List.exists(fun x -> x.Contains("mzlite"))
    validation
checkForMzLightFiles

let checkForPSMFiles = 
    let data = arc.Runs.[0].Tables.[2]
    let validation = data.OutputNames |> List.exists(fun x -> x.Contains("psm"))
    validation
checkForPSMFiles 

let checkForPSMSFiles = 
    let data = arc.Runs.[0].Tables.[3]
    let validation = data.OutputNames |> List.exists(fun x -> x.Contains("qpsm"))
    validation
checkForPSMSFiles 

let checkForQuantFiles = 
    let data = arc.Runs.[0].Tables.[4]
    let validation = data.OutputNames |> List.exists(fun x -> x.Contains("quant"))
    validation
checkForQuantFiles 

let checkForProtFiles = 
    let data = arc.Runs.[0].Tables.[5]
    let validation = data.OutputNames |> List.exists(fun x -> x.Contains("prot"))
    validation
checkForProtFiles 


//IDEE = such nach files im ARC z.B. runs -> psmstats falls vorhanden dann suchen wo dieses file in den
//metadaten vorhanden ist und parameter geben z.B. labeling 

let rec searchFiles (directoryName: string) (fileName: string) : string[] =
    // Files im aktuellen Verzeichnis
    let currentFiles : string[] =
        Directory.GetFiles(path = directoryName, searchPattern = fileName)

    // alle Subdirs
    let subDirectories : string[] =
        Directory.GetDirectories(path = directoryName)

    // rekursiv in Subdirs suchen
    let subDirFiles : string[] =
        subDirectories
        |> Array.collect (fun subDir -> searchFiles subDir fileName)

    // beides zusammenkleben
    Array.append currentFiles subDirFiles


let searchForPattern : string list = [
    "*.psm"
    "*.qpsm"
    "*.quant"
    "*.prot"
    "*.tsv"
    "*.txt"
]

let accessFiles (directoryName: string) : string[] =
    let directoryPath = "./runs" + directoryName

    searchForPattern
    |> List.map (fun pattern -> searchFiles directoryPath pattern) 
    |> List.toArray                                               
    |> Array.concat                                               

let execution = accessFiles "/dilutionSeriesChlamy_RUNS"
let validaaation = 
    if Array.isEmpty execution then printfn"no files detected"
    else printfn "files detected"
validaaation




    
