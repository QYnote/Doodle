using DotNet.Comm.Protocols;
using DotNet.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols.Customs.HYNux
{
    public class DataFrame_PCLink
    {
        public int DeviceAddr { get; set; }
        public string Command { get; set; }
        public object Target { get; set; }
        public object Value { get; set; }
    }

    public class PCLink : ProtocolFrame
    {
        public static readonly byte[] WhoCmd = new byte[] { 0x02, 0x30, 0x31, 0x57, 0x48, 0x4F, 0x0D, 0x0A, };
        public bool IsSUM { get; set; } = false;
        public bool IsTH3500 { get; set; } = false;
        public bool IsTD3500 { get; set; } = false;
        public byte[] TailBytes
        {
            get
            {
                if (this.IsTH3500)
                    //ETX + CR + LF
                    return new byte[] { 0x03, 0x0D, 0x0A };
                else
                    //CR + LF
                    return new byte[] { 0x0D, 0x0A };
            }
        }
        public List<string> CommandList
        {
            get
            {
                List<string> cmdList = new List<string>() { "WHO" };
                if (this.IsTD3500)
                {
                    cmdList.AddRange(new string[]
                    {
                        "WHO",
                        "RCS", "RPI",
                        "RLG", "RDR", "RRD", "RPD", "RSD",
                        "WLG", "WDR", "WRD", "WPD", "WSD",
                    });
                }
                else if (this.IsTH3500)
                {
                    cmdList.AddRange(new string[]
                    {
                        "WHO",
                        "RCS", "RCV",
                        "RSP", "RRP", "RUP", "RPD", "RSD", "RTD", "RLG",
                        "WSP", "WRP", "WUP", "WPD", "WSD", "WTD", "WLG",
                    });
                }
                else
                {
                    cmdList.AddRange(new string[]
                    {
                        "WHO",
                        "DWS", "DWR", "IWS", "IWR",
                        "DRS", "DRR", "IRS", "IRR",
                        "DMS", "DMC", "IMS", "IMC",
                    });
                }

                return cmdList;
            }
        }

        public PCLink(bool isClient) : base(isClient)
        {
            base.ErrCodeLength = 2;
        }

        #region Response

        public override byte[] Response_ExtractFrame(byte[] buffer, params object[] subData)
        {
            if (subData[0] == null) return null;
            byte[] reqFrame = subData[0] as byte[],
                   headerBytes;
            int startIdx,
                idxHandle = 0,
                endStartIdx = -1,
                endLastIdx = -1;

            //Header 정의
            if (this.IsTD3500)
                //STX[1] + Addr[3] + ','[1] + Cmd[3]
                headerBytes = new byte[] { reqFrame[0], reqFrame[1], reqFrame[2], reqFrame[3], reqFrame[4], reqFrame[5], reqFrame[6], reqFrame[7] };
            else if (this.IsTH3500)
                //STX[1] + Addr[3] + Cmd[3]
                headerBytes = new byte[] { reqFrame[0], reqFrame[1], reqFrame[2], reqFrame[3], reqFrame[4], reqFrame[5], reqFrame[6] };
            else
                //기본값: STX[1] + Addr[2] + Cmd[3]
                headerBytes = new byte[] { reqFrame[0], reqFrame[1], reqFrame[2], reqFrame[3], reqFrame[4], reqFrame[5] };

            if (buffer.Length < headerBytes.Length) return null;

            while (idxHandle < buffer.Length - 1)
            {
                //1. Header Receive 검사
                startIdx = Utils.Controls.Utils.QYUtils.Find(buffer, headerBytes, idxHandle++);
                if (startIdx < 0) continue;

                //2. Tail Receive 검사
                endStartIdx = Utils.Controls.Utils.QYUtils.Find(buffer, this.TailBytes, startIdx + 1);
                if (endStartIdx < 0) continue;

                endLastIdx = endStartIdx + this.TailBytes.Length - 1;
                if (this.IsTH3500 && this.IsSUM)
                    //ETX[1] + CRLF[2] + ErrCode[2]
                    endLastIdx += this.ErrCodeLength;
                if (buffer.Length < endLastIdx + 1) continue;

                //3. Frame 추출
                byte[] frameByte = new byte[endLastIdx - startIdx + 1];
                Buffer.BlockCopy(buffer, startIdx, frameByte, 0, frameByte.Length);

                return frameByte;
            }

            return null;
        }

        public override List<object> Response_ExtractData(byte[] frame, params object[] subData)
        {
            if (subData[0] == null) return null;
            byte[] reqBytes = subData[0] as byte[],
                   cmdBytes = new byte[3];

            if (this.IsTD3500)
                //STX[1] + Addr[3] + ','[1] + Cmd[3]
                Buffer.BlockCopy(frame, 5, cmdBytes, 0, 3);
            else if (this.IsTH3500)
                //STX[1] + Addr[3] + Cmd[3]
                Buffer.BlockCopy(frame, 4, cmdBytes, 0, 3);
            else
                //기본값: STX[1] + Addr[2] + Cmd[3]
                Buffer.BlockCopy(frame, 3, cmdBytes, 0, 3);
            string cmd = Encoding.ASCII.GetString(cmdBytes);
            if (cmd.StartsWith("NG"))
            {
                System.Diagnostics.Debug.WriteLine(string.Format("({0}) Protocol Error - {1}", this.GetType().Name, Encoding.ASCII.GetString(frame)));
                return null;
            }

            if (this.IsTH3500)
            {
                switch (cmd)
                {
                    case "WHO": return this.Get_WHO(cmd, frame);
                    case "RCS": break;
                    case "RCV": return this.Get_RCV(cmd, frame);
                    case "RSP": break;
                    case "RRP": return this.Get_RRP(cmd, frame);
                    case "RUP": return this.Get_RUP(cmd, frame);
                    case "RPD": break;
                    case "RSD": break;
                    case "RTD": return this.Get_RTD(cmd, frame);
                    case "RLG": break;
                    case "WSP": break;
                    case "WRP": break;
                    case "WUP": break;
                    case "WPD": break;
                    case "WSD": break;
                    case "WTD": break;
                    case "WLG": break;
                    default: System.Diagnostics.Debug.WriteLine(string.Format("({0}) Protocol Error - UnSupport Command: {1}", this.GetType().Name, cmd)); break;
                }
            }
            else if (this.IsTD3500)
            {
                switch (cmd)
                {
                    case "WHO": return this.Get_WHO(cmd, frame);
                    case "RCS": break;
                    case "RPI": break;
                    case "RLG": break;
                    case "RDR": return this.Get_RDR(cmd, frame, reqBytes);
                    case "RRD": return this.Get_RRD(cmd, frame, reqBytes);
                    case "RPD": break;
                    case "RSD": break;
                    case "WLG": break;
                    case "WDR": return this.Get_WDR(cmd, reqBytes);
                    case "WRD": return this.Get_WRD(cmd, reqBytes);
                    case "WPD": break;
                    case "WSD": break;
                    default: System.Diagnostics.Debug.WriteLine(string.Format("({0}) Protocol Error - UnSupport Command: {1}", this.GetType().Name, cmd)); break;
                }
            }
            else
            {
                switch (cmd)
                {
                    case "WHO": return this.Get_WHO(cmd, frame);
                    case "DRS": return this.Get_DRS(cmd, frame, reqBytes);
                    case "DRR": return this.Get_DRR(cmd, frame, reqBytes);
                    case "IRS": return this.Get_IRS(cmd, frame, reqBytes);
                    case "IRR": return this.Get_IRR(cmd, frame, reqBytes);
                    case "DWS": return this.Get_DWS(cmd, reqBytes);
                    case "DWR": return this.Get_DWR(cmd, reqBytes);
                    case "IWS": return this.Get_IWS(cmd, reqBytes);
                    case "IWR": return this.Get_IWR(cmd, reqBytes);
                    case "DMS": break;
                    case "DMC": break;
                    case "IMS": break;
                    case "IMC": break;
                    default: System.Diagnostics.Debug.WriteLine(string.Format("({0}) Protocol Error - UnSupport Command: {1}", this.GetType().Name, cmd)); break;
                }
            }

            return null;
        }

        #region Command Get Process

        /// <summary>
        /// WHO / 자기정보 읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        private List<object> Get_WHO(string cmd, byte[] frame)
        {
            List<object> list = new List<object>()
            {
                new DataFrame_PCLink()
                {
                    Command = cmd,
                    Target = null,
                    Value = Encoding.ASCII.GetString(frame)
                }
            };

            return list;
        }

        #region TH3,500 Command

        /// <summary>
        /// RRP / Read Run Parameter
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        private List<object> Get_RRP(string cmd, byte[] frame)
        {
            List<object> list = new List<object>();
            string strFrame = Encoding.ASCII.GetString(frame);
            string[] dataAry = strFrame.Split(',');

            int deviceAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { frame[1], frame[2], frame[3] })),
                regNo = Convert.ToInt32(dataAry[2]);

            if (regNo == 0)
            {
                for (int i = 0; i < 17; i++)
                {
                    list.Add(new DataFrame_PCLink()
                    {
                        DeviceAddr = deviceAddr,
                        Command = cmd,
                        Target = regNo + 1 + i,
                        Value = Convert.ToInt16(dataAry[3].Substring(0 + (i * 2), 2))
                    });
                }
            }
            else
            {
                list.Add(new DataFrame_PCLink()
                {
                    DeviceAddr = deviceAddr,
                    Command = cmd,
                    Target = regNo,
                    Value = Convert.ToInt16(dataAry[3].Substring(0, 2))
                });
            }

            return list;
        }
        /// <summary>
        /// RUP / Read User Parameter
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        private List<object> Get_RUP(string cmd, byte[] frame)
        {
            List<object> list = new List<object>();
            string strFrame = Encoding.ASCII.GetString(frame);
            string[] dataAry = strFrame.Split(',');

            int deviceAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { frame[1], frame[2], frame[3] })),
                regNo = Convert.ToInt32(dataAry[2]);

            if (regNo == 0)
            {
                for (int i = 0; i < 512; i++)
                {
                    list.Add(new DataFrame_PCLink()
                    {
                        DeviceAddr = deviceAddr,
                        Command = cmd,
                        Target = regNo + 1 + i,
                        Value = Convert.ToInt16(dataAry[3].Substring(0 + (i * 2), 2))
                    });
                }
            }
            else
            {
                list.Add(new DataFrame_PCLink()
                {
                    DeviceAddr = deviceAddr,
                    Command = cmd,
                    Target = regNo,
                    Value = Convert.ToInt16(dataAry[3].Substring(0, 2))
                });
            }

            return list;
        }
        /// <summary>
        /// RTD / Read Text Data
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        private List<object> Get_RTD(string cmd, byte[] frame)
        {
            List<object> list = new List<object>();
            int deviceAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { frame[1], frame[2], frame[3] })),
                regNo = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { frame[11], frame[12] }));
            byte[] txtTemp;

            if (12 <= regNo && regNo < 111)
            {
                //PatternName은 24byte만 사용
                txtTemp = new byte[24];
                Buffer.BlockCopy(frame, 16, txtTemp, 0, txtTemp.Length);

                list.Add(new DataFrame_PCLink()
                {
                    DeviceAddr = deviceAddr,
                    Command = cmd,
                    Target = regNo,
                    Value = Encoding.ASCII.GetString(txtTemp)
                });
            }
            else
            {
                txtTemp = new byte[29];
                Buffer.BlockCopy(frame, 16, txtTemp, 0, txtTemp.Length);

                list.Add(new DataFrame_PCLink()
                {
                    DeviceAddr = deviceAddr,
                    Command = cmd,
                    Target = regNo,
                    Value = Encoding.ASCII.GetString(txtTemp)
                });
            }

            return list;
        }
        /// <summary>
        /// RCV / Read Current Value
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        private List<object> Get_RCV(string cmd, byte[] frame)
        {
            List<object> list = new List<object>();
            int idxHandle = 11,
                deviceAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { frame[1], frame[2], frame[3] }));
            byte[] bytes;

            //TSV
            bytes = new byte[] { frame[idxHandle], frame[idxHandle + 1] };
            idxHandle += 2;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "TSV",
                Value = (Int16)(bytes[0] << 8 | bytes[1])
            });

            //TPV
            bytes = new byte[] { frame[idxHandle], frame[idxHandle + 1] };
            idxHandle += 2;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "TPV",
                Value = (Int16)(bytes[0] << 8 | bytes[1])
            });

            //TMV
            bytes = new byte[] { frame[idxHandle], frame[idxHandle + 1] };
            idxHandle += 2;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "TMV",
                Value = (Int16)(bytes[0] << 8 | bytes[1])
            });

            //HSV
            bytes = new byte[] { frame[idxHandle], frame[idxHandle + 1] };
            idxHandle += 2;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "HSV",
                Value = (Int16)(bytes[0] << 8 | bytes[1])
            });

            //HPV
            bytes = new byte[] { frame[idxHandle], frame[idxHandle + 1] };
            idxHandle += 2;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "HPV",
                Value = (Int16)(bytes[0] << 8 | bytes[1])
            });

            //HMV
            bytes = new byte[] { frame[idxHandle], frame[idxHandle + 1] };
            idxHandle += 2;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "HMV",
                Value = (Int16)(bytes[0] << 8 | bytes[1])
            });

            //T_I/S
            bytes = new byte[] { frame[idxHandle], frame[idxHandle + 1] };
            idxHandle += 2;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "T_I/S",
                Value = (Int16)(bytes[0] << 8 | bytes[1])
            });

            //T/S
            bytes = new byte[] { frame[idxHandle], frame[idxHandle + 1] };
            idxHandle += 2;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "T/S",
                Value = (Int16)(bytes[0] << 8 | bytes[1])
            });

            //A/S
            bytes = new byte[] { frame[idxHandle], frame[idxHandle + 1] };
            idxHandle += 2;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "A/S",
                Value = (Int16)(bytes[0] << 8 | bytes[1])
            });

            //RY
            bytes = new byte[] { frame[idxHandle], frame[idxHandle + 1] };
            idxHandle += 2;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "RY",
                Value = (Int16)(bytes[0] << 8 | bytes[1])
            });

            //O/C
            bytes = new byte[] { frame[idxHandle], frame[idxHandle + 1] };
            idxHandle += 2;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "O/C",
                Value = (Int16)(bytes[0] << 8 | bytes[1])
            });

            //D/I
            bytes = new byte[] { frame[idxHandle], frame[idxHandle + 1] };
            idxHandle += 2;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "D/I",
                Value = (Int16)(bytes[0] << 8 | bytes[1])
            });

            //RM
            bytes = new byte[] { frame[idxHandle] };
            idxHandle += 1;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "RM",
                Value = (Int16)bytes[0]
            });

            //RTH
            bytes = new byte[] { frame[idxHandle], frame[idxHandle + 1] };
            idxHandle += 2;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "RTH",
                Value = (Int16)(bytes[0] << 8 | bytes[1])
            });

            //RTS
            bytes = new byte[] { frame[idxHandle] };
            idxHandle += 1;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "RTS",
                Value = (Int16)bytes[0]
            });

            //SRTH
            bytes = new byte[] { frame[idxHandle], frame[idxHandle + 1] };
            idxHandle += 2;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "SRTH",
                Value = (Int16)(bytes[0] << 8 | bytes[1])
            });

            //SRTM
            bytes = new byte[] { frame[idxHandle] };
            idxHandle += 1;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "SRTM",
                Value = (Int16)bytes[0]
            });

            //SFTH
            bytes = new byte[] { frame[idxHandle], frame[idxHandle + 1] };
            idxHandle += 2;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "SFTH",
                Value = (Int16)(bytes[0] << 8 | bytes[1])
            });

            //SFTM
            bytes = new byte[] { frame[idxHandle] };
            idxHandle += 1;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "SFTM",
                Value = (Int16)bytes[0]
            });

            //RPTN
            bytes = new byte[] { frame[idxHandle], frame[idxHandle + 1] };
            idxHandle += 2;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "RPTN",
                Value = (Int16)(bytes[0] << 8 | bytes[1])
            });

            //RSEG
            bytes = new byte[] { frame[idxHandle] };
            idxHandle += 1;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "RSEG",
                Value = (Int16)bytes[0]
            });

            //RPRC
            bytes = new byte[] { frame[idxHandle], frame[idxHandle + 1] };
            idxHandle += 2;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "RPRC",
                Value = (Int16)(bytes[0] << 8 | bytes[1])
            });

            //RPRN
            bytes = new byte[] { frame[idxHandle], frame[idxHandle + 1] };
            idxHandle += 2;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "RPRN",
                Value = (Int16)(bytes[0] << 8 | bytes[1])
            });

            //RLC
            bytes = new byte[] { frame[idxHandle] };
            idxHandle += 1;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "RLC",
                Value = (Int16)bytes[0]
            });

            //RLN
            bytes = new byte[] { frame[idxHandle] };
            idxHandle += 1;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "RLN",
                Value = (Int16)bytes[0]
            });

            //UDSW
            bytes = new byte[] { frame[idxHandle] };
            idxHandle += 1;
            list.Add(new DataFrame_PCLink()
            {
                DeviceAddr = deviceAddr,
                Command = cmd,
                Target = "UDSW",
                Value = (Int16)bytes[0]
            });


            return list;
        }

        #endregion TH3,500 Command End
        #region TD3,500 Command

        /// <summary>
        /// RDR / Word Register Read, 연속읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        /// <param name="reqBytes">Request Data</param>
        private List<object> Get_RDR(string cmd, byte[] frame, byte[] reqBytes)
        {
            int deviceAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { frame[1], frame[2], frame[3] })),
                readCount = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[9], reqBytes[10] })),
                startAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[12], reqBytes[13], reqBytes[14], reqBytes[15] }));
            List<object> list = new List<object>();

            for (int i = 0; i < readCount; i++)
            {
                list.Add(new DataFrame_PCLink()
                {
                    DeviceAddr = deviceAddr,
                    Command = cmd,
                    Target = startAddr + i,
                    Value = Convert.ToInt16(Encoding.ASCII.GetString(new byte[] { frame[12 + (i * 5)], frame[13 + (i * 5)], frame[14 + (i * 5)], frame[15 + (i * 5)] }), 16)
                });
            }

            return list;
        }
        /// <summary>
        /// RRD / Word Register Read, 비연속읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        /// <param name="reqBytes">Request Data</param>
        private List<object> Get_RRD(string cmd, byte[] frame, byte[] reqBytes)
        {
            int deviceAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { frame[1], frame[2], frame[3] })),
                readCount = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[7], reqBytes[8] }));
            List<object> list = new List<object>();

            for (int i = 0; i < readCount; i++)
            {
                list.Add(new DataFrame_PCLink()
                {
                    DeviceAddr = deviceAddr,
                    Command = cmd,
                    Target = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[10 + (i * 5)], reqBytes[11 + (i * 5)], reqBytes[12 + (i * 5)], reqBytes[13 + (i * 5)] })),
                    Value = Convert.ToInt16(Encoding.ASCII.GetString(new byte[] { frame[10 + (i * 5)], frame[11 + (i * 5)], frame[12 + (i * 5)], frame[13 + (i * 5)] }), 16)
                });
            }

            return list;
        }
        /// <summary>
        /// WDR / Word Register Write, 연속쓰기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="reqBytes">Request Data</param>
        private List<object> Get_WDR(string cmd, byte[] reqBytes)
        {
            int deviceAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[1], reqBytes[2], reqBytes[3] })),
                readCount = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[9], reqBytes[10] })),
                startAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[12], reqBytes[13], reqBytes[14], reqBytes[15] }));
            List<object> list = new List<object>();

            for (int i = 0; i < readCount; i++)
            {
                list.Add(new DataFrame_PCLink()
                {
                    DeviceAddr = deviceAddr,
                    Command = cmd,
                    Target = startAddr + i,
                    Value = Convert.ToInt16(Encoding.ASCII.GetString(new byte[] { reqBytes[17 + (i * 5)], reqBytes[18 + (i * 5)], reqBytes[19 + (i * 5)], reqBytes[20 + (i * 5)] }), 16)
                });
            }

            return list;
        }
        /// <summary>
        /// WRD / Word Register Write, 비연속쓰기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="reqBytes">Request Data</param>
        private List<object> Get_WRD(string cmd, byte[] reqBytes)
        {
            int deviceAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[1], reqBytes[2], reqBytes[3] })),
                readCount = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[9], reqBytes[10] }));

            List<object> list = new List<object>();

            for (int i = 0; i < readCount; i++)
            {
                list.Add(new DataFrame_PCLink()
                {
                    DeviceAddr = deviceAddr,
                    Command = cmd,
                    Target = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[12 + (i * 10)], reqBytes[13 + (i * 10)], reqBytes[14 + (i * 10)], reqBytes[15 + (i * 10)] })),
                    Value = Convert.ToInt16(Encoding.ASCII.GetString(new byte[] { reqBytes[17 + (i * 10)], reqBytes[18 + (i * 10)], reqBytes[19 + (i * 10)], reqBytes[20 + (i * 10)] }), 16)
                });
            }

            return list;
        }

        #endregion TD3,500 Command End
        #region 기본 Command

        /// <summary>
        /// DRS / Word Register Read, 연속읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        /// <param name="reqBytes">Request Data</param>
        private List<object> Get_DRS(string cmd, byte[] frame, byte[] reqBytes)
        {
            int deviceAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { frame[1], frame[2] })),
                readCount = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[7], reqBytes[8] })),
                startAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[10], reqBytes[11], reqBytes[12], reqBytes[13] }));
            List<object> list = new List<object>();

            for (int i = 0; i < readCount; i++)
            {
                list.Add(new DataFrame_PCLink()
                {
                    DeviceAddr = deviceAddr,
                    Command = cmd,
                    Target = startAddr + i,
                    Value = Convert.ToInt16(Encoding.ASCII.GetString(new byte[] { frame[10 + (i * 5)], frame[11 + (i * 5)], frame[12 + (i * 5)], frame[13 + (i * 5)] }), 16)
                });
            }

            return list;
        }
        /// <summary>
        /// DRR / Word Register Read, 비연속읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        /// <param name="reqBytes">Request Data</param>
        private List<object> Get_DRR(string cmd, byte[] frame, byte[] reqBytes)
        {
            int deviceAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { frame[1], frame[2] })),
                readCount = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[7], reqBytes[8] }));
            List<object> list = new List<object>();

            for (int i = 0; i < readCount; i++)
            {
                list.Add(new DataFrame_PCLink()
                {
                    DeviceAddr = deviceAddr,
                    Command = cmd,
                    Target = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[10 + (i * 5)], reqBytes[11 + (i * 5)], reqBytes[12 + (i * 5)], reqBytes[13 + (i * 5)] })),
                    Value = Convert.ToInt16(Encoding.ASCII.GetString(new byte[] { frame[10 + (i * 5)], frame[11 + (i * 5)], frame[12 + (i * 5)], frame[13 + (i * 5)] }), 16)
                });
            }

            return list;
        }
        /// <summary>
        /// IRS / Bit Register Read, 연속읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        /// <param name="reqBytes">Request Data</param>
        private List<object> Get_IRS(string cmd, byte[] frame, byte[] reqBytes)
        {
            int deviceAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { frame[1], frame[2] })),
                readCount = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[7], reqBytes[8] })),
                startAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[10], reqBytes[11], reqBytes[12], reqBytes[13] }));
            List<object> list = new List<object>();

            for (int i = 0; i < readCount; i++)
            {
                list.Add(new DataFrame_PCLink()
                {
                    DeviceAddr = deviceAddr,
                    Command = cmd,
                    Target = startAddr + i,
                    Value = Encoding.ASCII.GetString(new byte[] { frame[15 + (i * 2)] }) == "1"
                });
            }

            return list;
        }
        /// <summary>
        /// IRS / Bit Register Read, 비연속읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        /// <param name="reqBytes">Request Data</param>
        private List<object> Get_IRR(string cmd, byte[] frame, byte[] reqBytes)
        {
            int deviceAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { frame[1], frame[2] })),
                readCount = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[7], reqBytes[8] }));
            List<object> list = new List<object>();

            for (int i = 0; i < readCount; i++)
            {
                list.Add(new DataFrame_PCLink()
                {
                    DeviceAddr = deviceAddr,
                    Command = cmd,
                    Target = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[10 + (i * 5)], reqBytes[11 + (i * 5)], reqBytes[12 + (i * 5)], reqBytes[13 + (i * 5)] })),
                    Value = Encoding.ASCII.GetString(new byte[] { frame[15 + (i * 2)] }) == "1"
                });
            }

            return list;
        }
        /// <summary>
        /// DWS / Word Register Write, 연속쓰기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="reqBytes">Request Data</param>
        private List<object> Get_DWS(string cmd, byte[] reqBytes)
        {
            int deviceAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[1], reqBytes[2] })),
                readCount = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[7], reqBytes[8] })),
                startAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[10], reqBytes[11], reqBytes[12], reqBytes[13] }));
            List<object> list = new List<object>();

            for (int i = 0; i < readCount; i++)
            {
                list.Add(new DataFrame_PCLink()
                {
                    DeviceAddr = deviceAddr,
                    Command = cmd,
                    Target = startAddr + i,
                    Value = Convert.ToInt16(Encoding.ASCII.GetString(new byte[] { reqBytes[15 + (i * 5)], reqBytes[16 + (i * 5)], reqBytes[17 + (i * 5)], reqBytes[18 + (i * 5)] }), 16)
                });
            }

            return list;
        }
        /// <summary>
        /// DWR / Word Register Write, 비연속쓰기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="reqBytes">Request Data</param>
        private List<object> Get_DWR(string cmd, byte[] reqBytes)
        {
            int deviceAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[1], reqBytes[2] })),
                readCount = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[7], reqBytes[8] }));
            List<object> list = new List<object>();

            for (int i = 0; i < readCount; i++)
            {
                list.Add(new DataFrame_PCLink()
                {
                    DeviceAddr = deviceAddr,
                    Command = cmd,
                    Target = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[10 + (i * 10)], reqBytes[11 + (i * 10)], reqBytes[12 + (i * 10)], reqBytes[13 + (i * 10)] })),
                    Value = Convert.ToInt16(Encoding.ASCII.GetString(new byte[] { reqBytes[15 + (i * 10)], reqBytes[16 + (i * 10)], reqBytes[17 + (i * 10)], reqBytes[18 + (i * 10)] }), 16)
                });
            }

            return list;
        }
        /// <summary>
        /// IWS / Bit Register Write, 연속쓰기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="reqBytes">Request Data</param>
        private List<object> Get_IWS(string cmd, byte[] reqBytes)
        {
            int deviceAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[1], reqBytes[2] })),
                readCount = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[7], reqBytes[8] })),
                startAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[10], reqBytes[11], reqBytes[12], reqBytes[13] }));
            List<object> list = new List<object>();

            for (int i = 0; i < readCount; i++)
            {
                list.Add(new DataFrame_PCLink()
                {
                    DeviceAddr = deviceAddr,
                    Command = cmd,
                    Target = startAddr + i,
                    Value = Encoding.ASCII.GetString(new byte[] { reqBytes[15 + (i * 2)] }) == "1"
                });
            }

            return list;
        }
        /// <summary>
        /// IWR / Bit Register Write, 비연속쓰기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="reqBytes">Request Data</param>
        private List<object> Get_IWR(string cmd, byte[] reqBytes)
        {
            int deviceAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[1], reqBytes[2] })),
                readCount = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[7], reqBytes[8] }));
            List<object> list = new List<object>();

            for (int i = 0; i < readCount; i++)
            {
                list.Add(new DataFrame_PCLink()
                {
                    DeviceAddr = deviceAddr,
                    Command = cmd,
                    Target = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { reqBytes[10 + (i * 7)], reqBytes[11 + (i * 7)], reqBytes[12 + (i * 7)], reqBytes[13 + (i * 7)] })),
                    Value = Encoding.ASCII.GetString(new byte[] { reqBytes[15 + (i * 7)] }) == "1"
                });
            }

            return list;
        }
        #endregion 기본 Command End

        #endregion Command Get Process End

        #endregion Response End
        #region Request

        public override byte[] Request_ExtractFrame(byte[] buffer, params object[] subData)
        {
            throw new NotImplementedException();
        }

        public override byte[] Request_CreateResponse(byte[] reqFrame, params object[] subData)
        {
            throw new NotImplementedException();
        }

        #endregion Request End
        #region ErrorCode

        public override bool ConfirmErrCode(byte[] frame)
        {
            if (this.IsSUM)
            {
                if (this.IsTH3500)
                    return this.ConfirmErrCode_TH(frame);
                else
                    return this.ConfirmErrCode_Normal(frame);
            }
            else
                return true;
        }
        
        #region ErrorCode 검사

        private bool ConfirmErrCode_Normal(byte[] bytes)
        {
            //STX[1] ~ (ErrCd[2] + CRLF[2])
            int sum = 0;
            int errStartIdx = bytes.Length - 4;
            byte[] chkCd;
            for (int i = 1; i < errStartIdx; i++)
            {
                sum += bytes[i];
            }

            byte[] sumBytes = BitConverter.GetBytes(sum);
            chkCd = Encoding.ASCII.GetBytes(Convert.ToString(sumBytes[0], 16).ToUpper());

            if (chkCd.Length == 1)
            {
                byte chkSumValue = chkCd[0];
                chkCd = new byte[2];
                chkCd[0] = 48;
                chkCd[1] = chkSumValue;
            }

            for (int i = 0; i < chkCd.Length; i++)
            {
                if (bytes[errStartIdx + i] != chkCd[i])
                {
                    return true;
                }
            }

            return false;
        }

        //ETX + CR + LF + CRC[2]
        private bool ConfirmErrCode_TH(byte[] bytes)
        {
            byte[] chkCd = new byte[2];
            int sum = 0;

            for (int i = 0; i < bytes.Length - 2; i++)
            {
                sum = (sum + bytes[i]) % 65536;
            }

            byte[] sumBytes = BitConverter.GetBytes(sum);
            chkCd[0] = sumBytes[1];
            chkCd[1] = sumBytes[0];

            for (int i = 0; i < chkCd.Length; i++)
            {
                if (bytes[bytes.Length - 2 + i] != chkCd[i])
                {
                    return true;
                }
            }

            return false;
        }

        #endregion ErrorCode 검사 End

        public override byte[] CreateErrCode(byte[] frame)
        {
            if (this.IsSUM)
            {
                if (this.IsTH3500)
                    return this.CreateErrCode_TH(frame);
                else
                    return this.CreateErrCode_Normal(frame);
            }
            else
                return null;
        }

        #region ErrorCode 생성

        private byte[] CreateErrCode_Normal(byte[] bytes)
        {
            byte[] chkCd;
            int sum = 0;

            for (int i = 1; i < bytes.Length; i++)
            {
                sum += bytes[i];
            }

            byte[] sumBytes = BitConverter.GetBytes(sum);
            chkCd = Encoding.ASCII.GetBytes(Convert.ToString(sumBytes[0], 16).ToUpper());

            if (chkCd.Length == 1)
            {
                byte chkSumValue = chkCd[0];
                chkCd = new byte[2];
                chkCd[0] = 48;
                chkCd[1] = chkSumValue;
            }

            return chkCd;
        }

        private byte[] CreateErrCode_TH(byte[] bytes)
        {
            byte[] chkCd = new byte[2];
            int sum = 0;

            for (int i = 0; i < bytes.Length; i++)
            {
                sum = (sum + bytes[i]) % 65536;
            }

            byte[] sumBytes = BitConverter.GetBytes(sum);
            chkCd[0] = sumBytes[1];
            chkCd[1] = sumBytes[0];

            return chkCd;
        }

        #endregion ErrorCode 생성 End

        #endregion ErrorCode End

        #region 일반 PCLink Request 생성

        /// <summary>
        /// IRR 랜덤 Address 읽기 Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public List<byte[]> CreateRequest_IRR(int deviceAddr, List<int> readList)
        {
            if (this.IsTH3500 || this.IsTD3500)
                throw new Exception(string.Format("[ERROR]CreateRequest - IRR : Protocol is TD3,500 or TH3,500"));

            //1. 연속 Adress목록 추출
            List<int[]> addrList = base.SortContinuouseAddress(readList, 32);
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addrAry in addrList)
            {
                if (addrAry.Length == 0) continue;

                //2. Main Frame 생성
                string bodyStr = string.Format("{0:D2}IRR,{1:D2}", deviceAddr, addrAry.Length);
                foreach (var addr in addrAry)
                    bodyStr += string.Format(",{0:D4}", addr);

                //3. Frame 생성
                byte[] frame = this.CreateFrame(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }
        /// <summary>
        /// DRS 연속 Address 읽기 Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public List<byte[]> CreateRequest_DRS(int deviceAddr, List<int> readList)
        {
            if (this.IsTH3500 || this.IsTD3500)
                throw new Exception(string.Format("[ERROR]CreateRequest - DRS : Protocol is TD3,500 or TH3,500"));

            //1. 연속 Adress목록 추출
            List<int[]> addrList = base.SortContinuouseAddress(readList, 32);
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addrAry in addrList)
            {
                if (addrAry.Length == 0) continue;

                //2. Main Frame 생성
                string bodyStr = string.Format("{0:D2}DRS,{1:D2},{2:D4}", deviceAddr, addrAry.Length, addrAry[0]);

                //3. Frame 생성
                byte[] frame = this.CreateFrame(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }
        /// <summary>
        /// DRR 랜덤 Address 읽기 Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public List<byte[]> CreateRequest_DRR(int deviceAddr, List<int> readList)
        {
            if (this.IsTH3500 || this.IsTD3500)
                throw new Exception(string.Format("[ERROR]CreateRequest - DRR : Protocol is TD3,500 or TH3,500"));

            //1. 연속 Adress목록 추출
            List<int[]> addrList = base.SortContinuouseAddress(readList, 32);
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addrAry in addrList)
            {
                if (addrAry.Length == 0) continue;

                //2. Main Frame 생성
                string bodyStr = string.Format("{0:D2}DRR,{1:D2}", deviceAddr, addrAry.Length);
                foreach (var addr in addrAry)
                    bodyStr += string.Format(",{0:D4}", addr);

                //3. Frame 생성
                byte[] frame = this.CreateFrame(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }

        #endregion 일반 PCLink Request 생성 End
        #region TD3,500 PCLink Request 생성

        /// <summary>
        /// RDR 연속 Address 읽기 Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public List<byte[]> CreateRequest_RDR(int deviceAddr, List<int> readList)
        {
            if (!this.IsTD3500)
                throw new Exception(string.Format("[ERROR]CreateRequest - RDR : Protocol is not TD3,500"));

            //1. 연속 Adress목록 추출
            List<int[]> addrList = base.SortContinuouseAddress(readList, 64);
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addrAry in addrList)
            {
                if (addrAry.Length == 0) continue;

                //2. Main Frame 생성
                string bodyStr = string.Format("{0:D3},RDR,{1:D2},{2:D4},", deviceAddr, addrAry.Length, addrAry[0]);

                //3. Frame 생성
                byte[] frame = this.CreateFrame(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }

        /// <summary>
        /// RRD 랜덤 Address 읽기 Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public List<byte[]> CreateRequest_RRD(int deviceAddr, List<int> readList)
        {
            if (!this.IsTD3500)
                throw new Exception(string.Format("[ERROR]CreateRequest - RRD : Protocol is not TD3,500"));

            //1. 연속 Adress목록 추출
            List<int[]> addrList = base.SortContinuouseAddress(readList, 64);
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addrAry in addrList)
            {
                if (addrAry.Length == 0) continue;

                //2. Main Frame 생성
                string bodyStr = string.Format("{0:D3},RRD,{1:D2}", deviceAddr, addrAry.Length);
                foreach (var addr in addrAry)
                    bodyStr += string.Format(",{0:D4}", addr);
                bodyStr += ",";

                //3. Frame 생성
                byte[] frame = this.CreateFrame(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }

        #endregion TD3,500 PCLink Request 생성
        #region TH3,500 PCLink Request 생성

        /// <summary>
        /// RSP / RRP / RUP, Data Address 읽기
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="cmd">명령어 Command</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public List<byte[]> CreateRequest_ReadAddress(int deviceAddr, string cmd, List<int> readList)
        {
            if (!this.IsTH3500)
                throw new Exception(string.Format("[ERROR]CreateRequest - ReadAddress : Protocol is not TH3,500"));
            if (readList.Count == 0) return null;
            if (cmd != "RSP" && cmd != "RRP" && cmd != "RUP")
                throw new Exception(string.Format("[ERROR]CreateRequest - ReadAddress : Protocol Command Error"));


            //1. 연속 Adress목록 추출
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addr in readList)
            {
                //2. Main Frame 생성
                string bodyStr = string.Format("{0:D3}{1},{2:D4}", deviceAddr, cmd, addr);

                //3. Frame 생성
                byte[] frame = this.CreateFrame_TH3500(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }
        /// <summary>
        /// RLG / RCS / RCV, Data Command 읽기
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="cmd">명령어 Command</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public List<byte[]> CreateRequest_Command(int deviceAddr, string cmd, List<int> readList)
        {
            if (!this.IsTH3500)
                throw new Exception(string.Format("[ERROR]CreateRequest - ReadCommand : Protocol is not TH3,500"));
            if (readList.Count == 0) return null;
            if (cmd != "RLG" && cmd != "RCS" && cmd != "RCV")
                throw new Exception(string.Format("[ERROR]CreateRequest - ReadCommand : Protocol Command Error"));


            //1. 연속 Adress목록 추출
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addr in readList)
            {
                //2. Main Frame 생성
                string bodyStr = string.Format("{0:D3}{1}", deviceAddr, cmd);

                //3. Frame 생성
                byte[] frame = this.CreateFrame_TH3500(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }

        #endregion TH3,500 PCLink Request 생성

        private byte[] CreateFrame(string dataStr)
        {
            byte[] frameBody = Encoding.ASCII.GetBytes(dataStr);

            //1. Header 입력
            byte[] frame = new byte[1 + frameBody.Length];
            frame[0] = 0x02;    //STX

            Buffer.BlockCopy(frameBody, 0, frame, 1, frameBody.Length);

            //2. SUM일경우 ErrorCode 입력
            if (this.IsSUM)
            {
                byte[] errCode = this.CreateErrCode(frame);
                byte[] tempErr = new byte[frame.Length + base.ErrCodeLength];

                Buffer.BlockCopy(frame, 0, tempErr, 0, frame.Length);
                Buffer.BlockCopy(errCode, 0, tempErr, frame.Length, base.ErrCodeLength);

                frame = tempErr;
            }

            //3. Tail 입력
            byte[] temp = new byte[frame.Length + this.TailBytes.Length];

            Buffer.BlockCopy(frame, 0, temp, 0, frame.Length);
            for (int i = 0; i < this.TailBytes.Length; i++)
                temp[temp.Length - this.TailBytes.Length + i] = this.TailBytes[i];

            frame = temp;

            return frame;
        }

        private byte[] CreateFrame_TH3500(string dataStr)
        {
            byte[] frameBody = Encoding.ASCII.GetBytes(dataStr);

            //1. Header 입력
            byte[] frame = new byte[1 + frameBody.Length];
            frame[0] = 0x02;    //STX

            Buffer.BlockCopy(frameBody, 0, frame, 1, frameBody.Length);

            //2. Tail 입력
            byte[] temp = new byte[frame.Length + this.TailBytes.Length];
            for (int i = 0; i < this.TailBytes.Length; i++)
                temp[temp.Length - this.TailBytes.Length + i] = this.TailBytes[i];

            frame = temp;

            //3. SUM일경우 ErrorCode 입력
            if (this.IsSUM)
            {
                byte[] errCode = this.CreateErrCode(frame);
                byte[] tempErr = new byte[frame.Length + base.ErrCodeLength];

                Buffer.BlockCopy(errCode, 0, tempErr, frame.Length, base.ErrCodeLength);

                frame = tempErr;
            }

            return frame;
        }
    }
}
