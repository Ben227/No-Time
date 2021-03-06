using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;

/// <summary>
/// Class that contains all Frame-manipulation Methodes
/// </summary>
public class FrameFormatter
{

	public string Path { get; set; }
	//if the formatter should Auto-serialze
	public bool running { get; set; }

	float UIC = 0;
	public float maxdur = 5;

	Queue<Frame> ToSerialize = new Queue<Frame> ();


	public FrameFormatter (string p)
	{
		Path = p;
	}

	/// <summary>
	/// Serializes the Frame f
	/// it gets provided
	/// </summary>
	/// <param name="f">Serialize this frame</param>
	public void Serialize (Frame f)
	{
		//Debug.Log ("started Serializatione");
		FileStream fs = new FileStream (Path, FileMode.Append);
		try {
			BinaryFormatter bf = new BinaryFormatter ();
			bf.Serialize (fs, f);
			Debug.Log ("Succsesfully serialized Data: " + f.FrameID);
		} catch (Exception ex) {
			Debug.LogError (ex.ToString ());
		} finally {
			fs.Close ();
		}
	}

	/// <summary>
	/// Adds the frame to queue.
	/// </summary>
	/// <param name="f">Frame</param>
	public void AddFrameToQueue (Frame f)
	{
		ToSerialize.Enqueue (f);
	}

	/// <summary>
	/// Serializes a list of Frames
	/// </summary>
	/// <param name="fs">FrameList</param>
	public void SerializeList (List<Frame> fs)
	{
		foreach (Frame item in fs) {
			AddFrameToQueue (item);
		}
	}

	/// <summary>
	/// Deserializes the set path and returns
	/// a List of frames
	/// </summary>
	/// <returns>list of all Frames</returns>
	public List<Frame> DeSerialize ()
	{
		
		List<Frame> frames = new List<Frame> ();
		if (File.Exists (Path)) {
			FileStream fs = new FileStream (Path, FileMode.Open);

			try {			
				BinaryFormatter bf = new BinaryFormatter ();
				try {
					while (fs.Position < fs.Length) {
						Frame f = (Frame)bf.Deserialize (fs);
						frames.Add (f);
					}
				} catch (Exception ex) {
					Debug.LogError (ex.ToString ());
				}
			} catch (Exception ex) {
				Debug.LogError (ex.ToString ());
			}
			fs.Close ();
		}

		return frames;
	}

	/// <summary>
	/// Deletes File in Path
	/// if File exists
	/// </summary>
	/// <returns><c>true</c>, if file was deleted, <c>false</c> otherwise.</returns>
	public bool DelPath ()
	{
		try {
			File.Delete (Path);
		} catch (Exception) {
			return false;
		}
		return true;
	}


	/// <summary>
	/// Runs the auto serialization.
	/// </summary>
	public void RunAutoSerialization ()
	{
		while (running) {
			if (ToSerialize.Count > 0) {
				Frame f = ToSerialize.Dequeue ();
				if (f.FrameID < 0) {
					SaveHistoryUntilFrameId (-f.FrameID);
					Debug.Log ("deserialize");
				} else {
					Serialize (f);
				}
				UIC = 0;
			} else {
				//Debug.Log ("wait");
				UIC = UIC + 1;
				if (UIC > maxdur) {
					Debug.Log ("I Died right now!");
					Thread.CurrentThread.Abort ();
				}
				System.Threading.Thread.Sleep (250);
			}
		}
		Debug.Log ("I ran out of stuff to do!");
	}

	/// <summary>
	/// Saves the history until frame identifier.
	/// </summary>
	/// <returns><c>true</c>, if history until frame identifier was saved, <c>false</c> if not</returns>
	/// <param name="Id">Identifier of last frame</param>
	public bool SaveHistoryUntilFrameId (int Id)
	{
		List<Frame> now = new List<Frame> ();
		List<Frame> old = DeSerialize ();
		foreach (Frame item in old) {
			if (item.FrameID <= Id) {
				now.Add (item);
			}
		}
		if (now.Count < 1) {
			return false;
		} else {
			DelPath ();
			SerializeList (now);
			return true;
		}

	}




}
