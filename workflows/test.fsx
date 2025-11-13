//test CQC for QualIQon - starting with getting accses of data stored in the ARC 

#r "nuget: ARCtrl"

open ARCtrl

let arc = ARC()
let arcPath = @"/home/paulinehans/Dokumente/TestARCForQualIQon"
let loadingARC = ARC.load(arcPath)

let writing = arc.Write("/home/paulinehans/Dokumente/test")




 