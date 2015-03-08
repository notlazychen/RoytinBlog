using System;
using System.Collections.Generic;

using iBoxDB.LocalServer;
using iBoxDB.LocalServer.IO;


/*
Windows Store Config
VS: 
public MainPage()
{
    iBoxDB.WSDatabaseConfig.ResetStorage();
    this.InitializeComponent();
    ...
}
---- 
Unity3D:
  /Plugins/iBoxDB.net2.dll
  /WSDatabaseConfig.cs
  /Script.cs
  void Start () {
    iBoxDB.WSDatabaseConfig.ResetStorage();
    ...
  } 
  Type->C#, SDK->8.1, Build->Open VS2013->X86->Ctrl+F5
*/
#if (UNITY_METRO || NETFX_CORE) && (!UNITY_EDITOR)
using Windows.Storage;
using Windows.Storage.Streams;


namespace iBoxDB
{

    public class WSDatabaseConfig : DatabaseConfig
    {
        public static void ResetStorage()
        {
            BoxSystem.Platform.DeleteFile = (path) =>
            {
                WSDatabaseConfig.DeleteFile(path);
                return true;
            };
            BoxFileStreamConfig.AdapterType = typeof(WSDatabaseConfig);
        }

        public override void Dispose()
        {
            if (streams != null)
            {
                foreach (var s in streams)
                {
                    s.Value.Dispose();
                }
                streams = null;
            }
            base.Dispose();
        }
        Dictionary<string, IRandomAccessStream> streams = new Dictionary<string, IRandomAccessStream>();
        public override IBStream CreateStream(string path, StreamAccess access)
        {
            IRandomAccessStream s;
            if (!streams.TryGetValue(path, out s))
            {
                var x = ApplicationData.Current.LocalFolder.CreateFileAsync(path, Windows.Storage.CreationCollisionOption.OpenIfExists).AsTask();
                x.Wait();
                var y = x.Result.OpenAsync(FileAccessMode.ReadWrite).AsTask();
                y.Wait();
                s = y.Result;
                streams.Add(path, s);
            }
            return new LocalStream(s);
        }
        public override bool ExistsStream(string path)
        {
            try
            {
                var x = ApplicationData.Current.LocalFolder.GetFileAsync(path).AsTask();
                x.Wait();
                //return x.Result.IsAvailable;
 		return true;
            }
            catch
            {
                return false;
            }
        }
        public static void DeleteFile(string path)
        {
            try
            {
                var x = ApplicationData.Current.LocalFolder.GetFileAsync(path).AsTask();
                x.Wait();
                x.Result.DeleteAsync().AsTask().Wait();
            }
            catch
            {

            }
        }

        class LocalStream : IBStream
        {
            private IRandomAccessStream rs;

            public LocalStream(IRandomAccessStream randomAccessStream)
            {
                // TODO: Complete member initialization
                this.rs = randomAccessStream;
            }


            public void SetLength(long value)
            {
                rs.Size = (ulong)value;
            }

            public int Read(long position, byte[] buffer, int offset, int count)
            {
                using (var os = rs.GetInputStreamAt((ulong)position))
                {
                    using (DataReader dr = new DataReader(os))
                    {
                        try
                        {
                            dr.LoadAsync((uint)count).AsTask().Wait();
                            byte[] bs = new byte[dr.UnconsumedBufferLength];
                            dr.ReadBytes(bs);
                            System.Buffer.BlockCopy(bs, 0, buffer, offset, bs.Length);
                            dr.DetachStream();
                            return bs.Length;
                        }
                        catch
                        {
                            return 0;
                        }
                    }
                }
            }

            public void BeginWrite(long appID, int maxLen)
            {

            }

            delegate void WaitAction();
            List<WaitAction> acts = new List<WaitAction>();
            public void Write(long position, byte[] buffer, int offset, int count)
            {
                var os = rs.GetOutputStreamAt((ulong)position);
                {
                    DataWriter dw = new DataWriter(os);
                    {
                        var bs = new byte[count];
                        System.Buffer.BlockCopy(buffer, offset, bs, 0, count);
                        dw.WriteBytes(bs);
                        acts.Add(dw.StoreAsync().AsTask().Wait);
                        acts.Add(dw.FlushAsync().AsTask().Wait);
                        acts.Add(() => { dw.DetachStream(); });
                    }
                    acts.Add(dw.Dispose);
                }
                acts.Add(os.FlushAsync().AsTask().Wait);
                acts.Add(os.Dispose);

            }

            public void EndWrite()
            {

            }

            public void Flush()
            {
                if (acts.Count > 0)
                {
                    acts.Add(rs.FlushAsync().AsTask().Wait);
                    foreach (var w in acts) { w(); }
                }
                acts.Clear();
            }

            public long Length
            {
                get { return (long)rs.Size; }
            }

            public void Dispose()
            {
            }
        }
    }
}
#endif