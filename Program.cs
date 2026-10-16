using System;
using System.IO;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

if (args.Length == 0) { Console.WriteLine("Drag one or more GLB files onto this program."); if (!Console.IsInputRedirected) Console.ReadKey(); return; }
foreach (var arg in args) try { Fix(arg); } catch (Exception e) { Console.WriteLine($"Failed: {arg}\n  {e.Message}"); }
Console.WriteLine("Finished. Press any key to close."); if (!Console.IsInputRedirected) Console.ReadKey();

static void Fix(string path) {
  var raw=File.ReadAllBytes(path);
  if(raw.Length<28 || Encoding.ASCII.GetString(raw,0,4)!="glTF" || BinaryPrimitives.ReadUInt32LittleEndian(raw.AsSpan(4,4))!=2) throw new Exception("Not a GLB 2.0 file");
  int jl=(int)BinaryPrimitives.ReadUInt32LittleEndian(raw.AsSpan(12,4));
  var doc=JsonNode.Parse(raw.AsSpan(20,jl))!.AsObject(); int bin=28+jl, fixes=0;
  var acc=doc["accessors"]!.AsArray(); var views=doc["bufferViews"]!.AsArray();
  int mi=0; foreach(var mn in doc["meshes"]!.AsArray()) {
    var prims=mn!["primitives"]!.AsArray();
    for(int ri=prims.Count-1;ri>=0;ri--){
      var rp=prims[ri]!.AsObject(); var ra=rp["attributes"]?.AsObject();
      if(ra?["POSITION"] is null) continue;
      int pc=acc[ra["POSITION"]!.GetValue<int>()]!["count"]!.GetValue<int>(); bool mismatch=false;
      foreach(var kv in ra) if(acc[kv.Value!.GetValue<int>()]!["count"]!.GetValue<int>()!=pc){mismatch=true;break;}
      if(mismatch){Console.WriteLine($"Removed mesh {mi} primitive {ri}: inconsistent vertex attribute counts");prims.RemoveAt(ri);fixes++;}
    }
    int pi=0; foreach(var pn in prims) {
    var p=pn!.AsObject(); var attrs=p["attributes"]?.AsObject(); if(p["indices"] is null||attrs?["POSITION"] is null){pi++;continue;}
    int ii=p["indices"]!.GetValue<int>(), po=attrs["POSITION"]!.GetValue<int>(); var ia=acc[ii]!.AsObject(); var pa=acc[po]!.AsObject();
    int ct=ia["componentType"]!.GetValue<int>(); if(ct is not (5121 or 5123 or 5125)){pi++;continue;} int sz=ct==5121?1:ct==5123?2:4;
    var bv=views[ia["bufferView"]!.GetValue<int>()]!.AsObject(); int stride=bv["byteStride"]?.GetValue<int>()??sz;
    int start=bin+(bv["byteOffset"]?.GetValue<int>()??0)+(ia["byteOffset"]?.GetValue<int>()??0), count=ia["count"]!.GetValue<int>(), verts=pa["count"]!.GetValue<int>(), bad=-1;
    for(int i=0;i<count;i++){var s=raw.AsSpan(start+i*stride,sz); uint v=sz==1?s[0]:sz==2?BinaryPrimitives.ReadUInt16LittleEndian(s):BinaryPrimitives.ReadUInt32LittleEndian(s);if(v>=verts){bad=i;break;}}
    if(bad>=0){int safe=bad-bad%3;ia["count"]=safe;Console.WriteLine($"Repaired mesh {mi} primitive {pi}: {count} -> {safe}");fixes++;} pi++;
  } mi++; }
  if(fixes==0){Console.WriteLine($"OK, no repair needed: {path}");return;}
  var json=Encoding.UTF8.GetBytes(doc.ToJsonString(new JsonSerializerOptions{WriteIndented=false}));
  int padded=(json.Length+3)&~3, oldBinHeader=20+jl, tail=raw.Length-oldBinHeader;
  var output=new byte[20+padded+tail];
  Array.Copy(raw,0,output,0,12);
  BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(8,4),(uint)output.Length);
  BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(12,4),(uint)padded);
  BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(16,4),0x4E4F534A);
  Array.Copy(json,0,output,20,json.Length);Array.Fill(output,(byte)0x20,20+json.Length,padded-json.Length);
  Array.Copy(raw,oldBinHeader,output,20+padded,tail);
  var dst=Path.Combine(Path.GetDirectoryName(path)!,Path.GetFileNameWithoutExtension(path)+"_fixed.glb");File.WriteAllBytes(dst,output);Console.WriteLine($"OK, repaired file saved: {dst}");
}

