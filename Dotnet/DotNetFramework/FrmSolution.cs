using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFramework
{
    public partial class FrmSolution : Form
    {
        private ToolStripButton btnCommTester;
        private ToolStripButton btnSensorToImage;
        private ToolStripButton btnTest;

        public FrmSolution()
        {
            InitializeComponent();

            this.IsMdiContainer = true;
            this.Text = ".Net FrameWork(WinForm)";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1200, 800);

            this.IsMdiContainer = true;

            InitUI();
        }

        private void InitUI()
        {
            //MenuBar
            ToolStrip TopMenu = new ToolStrip();
            TopMenu.ImageScalingSize = new Size(32, 32);
            TopMenu.ItemClicked += (sender, e) => { MdiOpen(e.ClickedItem.Name); };

            this.btnCommTester = new ToolStripButton() { DisplayStyle = ToolStripItemDisplayStyle.Image }; //통신
            this.btnCommTester.Name = "CommTester";
            this.btnCommTester.Image = Dnf.Utils.Properties.Resources.Connect_32x32;
            this.btnCommTester.ToolTipText = "통신테스터기";

            this.btnSensorToImage = new ToolStripButton() { DisplayStyle = ToolStripItemDisplayStyle.Image }; //통신
            this.btnSensorToImage.Name = "SensorToImage";
            this.btnSensorToImage.Image = Dnf.Utils.Properties.Resources.Image_32x32;
            this.btnSensorToImage.ToolTipText = "센서이미지화";

            this.btnTest = new ToolStripButton() { DisplayStyle = ToolStripItemDisplayStyle.Image }; //통신
            this.btnTest.Name = "Test";
            this.btnTest.Image = Dnf.Utils.Properties.Resources.Test_32x32;
            this.btnTest.ToolTipText = "테스트";

            //메뉴 추가
            TopMenu.Items.AddRange(new ToolStripItem[] {
                this.btnCommTester,
                this.btnSensorToImage,
                this.btnTest
            });

            this.Controls.Add(TopMenu);
        }

        private void MdiOpen(string btnName)
        {
            Form frm = null;
            bool isOpen = false;

            //이미 열린 Form인지 탐색
            foreach (Form frmChild in this.MdiChildren)
            {
                if (btnName == frmChild.Name)
                {
                    isOpen = true;
                    frmChild.Focus();
                    break;
                }
            }

            if (!isOpen)
            {
                //Form 생성
                if (btnName == btnCommTester.Name) { frm = new Dnf.Comm.Frm.MainForm() { Name = btnCommTester.Name }; }
                else if (btnName == btnSensorToImage.Name) { frm = new Dnf.DrawImage.FrmMain_DrawImage() { Name = btnSensorToImage.Name }; }
                else if (btnName == btnTest.Name)
                {
                    frm = null;
                }

                //이미 틀어져 있는지 검색
                if (frm != null)
                {
                    frm.MdiParent = this;
                    frm.WindowState = FormWindowState.Maximized;
                    frm.Show();
                }
            }
        }

        class Receipe
        {
            public string Name = string.Empty;
            public decimal MaxQty = 0;
            public decimal CreQty = 0;
            public Dictionary<Material, decimal> Materials = new Dictionary<Material, decimal>();
        }
        class Material
        {
            public string Name = string.Empty;
            public decimal Price = 0;
            public decimal MaxWeekQty = 0;
            public decimal CurWeekQty = 0;
        }
        Dictionary<string, Material> dicMat;

        private void temp()
        {
            Receipe[] receipes = new Receipe[6];
            dicMat = new Dictionary<string, Material>();
            dicMat.Add("소금", new Material() { Name = "소금", MaxWeekQty = 60, Price = 100 });
            dicMat.Add("설탕", new Material() { Name = "설탕", MaxWeekQty = 60, Price = 1200 });
            dicMat.Add("양배추", new Material() { Name = "양배추", MaxWeekQty = 60, Price = 800 });
            dicMat.Add("식용유", new Material() { Name = "식용유", MaxWeekQty = 60, Price = 1200 });
            dicMat.Add("마늘", new Material() { Name = "마늘", MaxWeekQty = 30, Price = 1200 });
            dicMat.Add("후추", new Material() { Name = "후추", MaxWeekQty = 60, Price = 2000 });
            dicMat.Add("레몬", new Material() { Name = "레몬", MaxWeekQty = 30, Price = 3000 });
            dicMat.Add("토마토", new Material() { Name = "토마토", MaxWeekQty = 60, Price = 6800 });
            dicMat.Add("아스파라거스", new Material() { Name = "아스파라거스", MaxWeekQty = 60, Price = 10000 });
            dicMat.Add("완두콩", new Material() { Name = "완두콩", MaxWeekQty = 60, Price = 10000 });
            dicMat.Add("고기", new Material() { Name = "고기", MaxWeekQty = 60, Price = 250 });
            dicMat.Add("딸기", new Material() { Name = "딸기", MaxWeekQty = 30, Price = 1200 });

            dicMat["소금"].MaxWeekQty -= dicMat["소금"].MaxWeekQty;
            dicMat["설탕"].MaxWeekQty -= dicMat["설탕"].MaxWeekQty;
            dicMat["양배추"].MaxWeekQty -= dicMat["양배추"].MaxWeekQty;
            dicMat["식용유"].MaxWeekQty -= dicMat["식용유"].MaxWeekQty;
            dicMat["마늘"].MaxWeekQty -= dicMat["마늘"].MaxWeekQty;
            dicMat["후추"].MaxWeekQty -= dicMat["후추"].MaxWeekQty;
            dicMat["레몬"].MaxWeekQty -= dicMat["레몬"].MaxWeekQty;
            dicMat["토마토"].MaxWeekQty -= dicMat["토마토"].MaxWeekQty;
            dicMat["아스파라거스"].MaxWeekQty -= dicMat["아스파라거스"].MaxWeekQty;
            dicMat["완두콩"].MaxWeekQty -= dicMat["완두콩"].MaxWeekQty;
            dicMat["고기"].MaxWeekQty -= dicMat["고기"].MaxWeekQty;
            dicMat["딸기"].MaxWeekQty -= dicMat["딸기"].MaxWeekQty;

            //현재 가지고있는 재료 수
            dicMat["레몬"].MaxWeekQty += 1 + 30;
            dicMat["고기"].MaxWeekQty += 64;
            dicMat["마늘"].MaxWeekQty += 3;
            dicMat["후추"].MaxWeekQty += 1;
            dicMat["설탕"].MaxWeekQty += 8;
            dicMat["딸기"].MaxWeekQty += 30;

            for (int i = 0; i < receipes.Length; i++)
            {
                receipes[i] = new Receipe();

                switch (i)
                {
                    case 0:
                        receipes[i].Name = "조개찜";
                        receipes[i].Materials.Add(dicMat["레몬"], 4);
                        break;
                    case 1:
                        receipes[i].Name = "크림소스 스테이크";
                        receipes[i].Materials.Add(dicMat["고기"], 10);
                        receipes[i].Materials.Add(dicMat["마늘"], 2);
                        receipes[i].Materials.Add(dicMat["후추"], 3);
                        receipes[i].Materials.Add(dicMat["설탕"], ((decimal)2 / 3) * 2);
                        break;
                    case 2:
                        receipes[i].Name = "감자수프";
                        receipes[i].Materials.Add(dicMat["후추"], 6);
                        break;
                    case 3:
                        receipes[i].Name = "알리오 올리오";
                        receipes[i].Materials.Add(dicMat["마늘"], 7);
                        receipes[i].Materials.Add(dicMat["후추"], 2);

                        break;
                    case 4:
                        receipes[i].Name = "얼음 딸기주스";
                        receipes[i].Materials.Add(dicMat["딸기"], 6);
                        receipes[i].Materials.Add(dicMat["설탕"], 4);
                        break;
                    case 5:
                        receipes[i].Name = "사과 생크림케이크";
                        receipes[i].Materials.Add(dicMat["설탕"], 11);
                        break;
                }

                foreach (var matPair in receipes[i].Materials)
                {
                    //주간 최대 제작량
                    decimal qty = matPair.Key.MaxWeekQty / matPair.Value;

                    if (receipes[i].MaxQty < qty) receipes[i].MaxQty = qty;
                }
            }

            List<List<int>> list = new List<List<int>>();
            List<int> idxs = new List<int>();

            for (int i = 0; i < receipes.Length; i++)
            {
                idxs.Add(i);
            }

            Permute(idxs, 0, list);

            decimal bestCnt = 0;
            int handle = 0;
            foreach (var ary in list)
            {
                decimal maxTotalCnt = 0;
                //Debug.WriteLine(handle);

                //레시피 제작Idx 순서
                foreach (var idx in ary)
                {
                    handle++;
                    //생산 음식
                    Receipe receip = receipes[idx];
                    int receipMaxCnt = 999;

                    foreach (var mPair in receip.Materials)
                    {
                        Material material = mPair.Key;
                        decimal consumeQty = mPair.Value;
                        
                        //남은 주간 최대 제작 수
                        if (material.MaxWeekQty - material.CurWeekQty >= consumeQty)
                        {
                            int matCreQty = (int)((material.MaxWeekQty - material.CurWeekQty) / consumeQty);

                            //주간 최대 제작 수만큼 주간 남은 재료수 계산처리
                            if (receipMaxCnt > matCreQty)
                            {
                                receipMaxCnt = matCreQty;

                                material.CurWeekQty += (receipMaxCnt * consumeQty);
                                receip.CreQty = receipMaxCnt;
                            }
                        }
                        else
                        {
                            receipMaxCnt = 0;
                            receip.CreQty = receipMaxCnt;
                            break;
                        }
                    }

                    if(receipMaxCnt != 999)
                        maxTotalCnt += receipMaxCnt;
                }

                if(bestCnt < maxTotalCnt)
                {
                    bestCnt = maxTotalCnt;
                    Debug.WriteLine(string.Format("Ary:{0} - Count:{1} - Handle:{2}", string.Join("\t", ary), bestCnt, handle));
                    foreach (var idx in ary)
                    {
                        Debug.WriteLine(string.Format("Receipe:{0} - Count:{1}", receipes[idx].Name, receipes[idx].CreQty));
                    }
                }

                //초기화
                foreach (var idx in ary)
                {
                    foreach (var material in receipes[idx].Materials.Keys)
                        material.CurWeekQty = 0;

                    receipes[idx].CreQty = 0;
                }
            }
        }

        private void Permute(List<int> list, int startIdx, List<List<int>> rst)
        {
            if (startIdx == list.Count)
            {
                rst.Add(new List<int>(list));
                return;
            }

            for (int i = startIdx; i < list.Count; i++)
            {
                QYUtils.Swap(list, startIdx, i);
                Permute(list, startIdx + 1, rst);
                QYUtils.Swap(list, startIdx, i);
            }
        }
    }
}
