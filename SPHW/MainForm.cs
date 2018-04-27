using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SemaphoreHW
{
    public partial class MainForm : Form
    {
        List<Thread> createdThreads, waitingThreads, workingThreads;
        Semaphore semaphore;
        static int counterTh = 0,maxAmount = 3;
        Dictionary<Thread, int> dictCounterTh;

        private void ListView1_DoubleClick(object sender, EventArgs e)
        {
            string number = (sender as ListView).SelectedItems[0].Text.Replace("Создан поток  --> ","");
            (sender as ListView).SelectedItems[0].Remove();
            Thread th = createdThreads.First(x => x.ManagedThreadId.ToString() == number);
            createdThreads.Remove(th);
            waitingThreads.Add(th);
            listView2.Items.Add($"Ожидает поток --> {th.ManagedThreadId}");

        }

        private void ListView2_DoubleClick(object sender, EventArgs e)
        {
            string number = (sender as ListView).SelectedItems[0].Text.Replace("Ожидает поток --> ", "");
            Thread th = waitingThreads.First(x => x.ManagedThreadId.ToString() == number);
            if (counterTh < maxAmount)
            {
                (sender as ListView).SelectedItems[0].Remove();
                waitingThreads.Remove(th);
                workingThreads.Add(th);
                th.Start();
                listView3.Items.Add(workingThreads.IndexOf(th).ToString(), $"Работает поток -->{th.ManagedThreadId}-->0", "");
            }
        }

        public MainForm()
        {
            InitializeComponent();
            createdThreads = new List<Thread>();
            waitingThreads = new List<Thread>();
            workingThreads = new List<Thread>();
            semaphore = new Semaphore(maxAmount, maxAmount);
            listView1.Columns.Add("Потоки");
            listView2.Columns.Add("Потоки");
            listView3.Columns.Add("Потоки");
            dictCounterTh = new Dictionary<Thread, int>();
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            workingThreads.ForEach(x => x.Suspend());
            semaphore.Close();
            int prevMax = maxAmount;
            maxAmount = (int)(sender as NumericUpDown).Value;
            semaphore = new Semaphore(maxAmount, maxAmount);
            if(maxAmount > prevMax)
            {
                workingThreads.ForEach(x => x.Resume());
            }
            else
            {
                int countRelease = maxAmount - prevMax; 
                var threads = dictCounterTh.OrderBy(x => x.Value).ToList();
                for (int i = 0; i < countRelease; i++)
                {
                    workingThreads.Remove(threads[i].Key);
                }
                workingThreads.ForEach(x=>x.Resume());
            }
        }

        private void ListView3_DoubleClick(object sender, EventArgs e)
        {
            string number = (sender as ListView).
                SelectedItems[0].Text
                .Replace("Работает поток -->", "")
                .Replace("-->","");
            number = number.Remove(number.Length - 2);
            (sender as ListView).SelectedItems[0].Remove();
            Thread th = workingThreads.FirstOrDefault(x => x.ManagedThreadId.ToString() == number);
            th.Abort();
            workingThreads.Remove(th);
        }

        private void CreateThreadButton_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(Incrementer);
            createdThreads.Add(th);
            listView1.Items.Add($"Создан поток  --> {th.ManagedThreadId}");
        }
        private void Incrementer()
        {
            semaphore.WaitOne();
            counterTh++;
            Thread curThread = Thread.CurrentThread;
            int counter = 0;
            dictCounterTh.Add(curThread, counter);
            while (curThread.ThreadState != ThreadState.AbortRequested)
            {
                listView3.Invoke(new Action(() =>
                {
                    listView3.Items.
                      Find(workingThreads.
                      IndexOf(curThread).
                      ToString(), false).
                      FirstOrDefault().
                      Text = $"Работает поток -->{curThread.ManagedThreadId}-->{counter}";
                }));
                dictCounterTh[curThread] = counter;
                counter++;
                Thread.Sleep(1000);
            }
            semaphore.Release();
        }
    }
}
