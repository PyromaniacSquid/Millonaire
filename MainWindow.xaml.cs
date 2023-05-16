using Microsoft.Win32;
using System;
using System.Collections.Generic;
//using NETCore.Encrypt;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Numerics;
using System.Windows;
using System.Windows.Media;

namespace Millonaire
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int max_num;
        private bool userA_ready = false;
        private bool userB_ready = false;
        private int A_value;
        private int B_value;
        private bool detailed_mode = false;
        private int current_stage = 0;
        // Cryptographic vars
        BigInteger q;
        int generator = 2;


        // Для детального вида
        struct Stagelog
        {
            public string A_Actions;
            public string B_Actions;
        }
        private List<Stagelog> Log;
        public MainWindow()
        {
            InitializeComponent();
            GridA.IsEnabled = GridB.IsEnabled = false;
            GridA.Visibility = GridB.Visibility = Visibility.Hidden;
            DetailedViewA.Visibility = DetailedViewB.Visibility = Visibility.Hidden;
            DetailedButtonsGrid.Visibility = Visibility.Hidden;
            Log = new List<Stagelog>();
        }

        private void TurnDetailedViewGenerator(bool turnOn)
        {
            if (turnOn)
            {
                q = 227;
                generator = 2;
            }
            else
            {
                string hex = "0FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B139B22514A08798E3404DDEF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7EDEE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3DC2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F83655D23DCA3AD961C62F356208552BB9ED529077096966D670C354E4ABC9804F1746C08CA237327FFFFFFFFFFFFFFFF";
                q = BigInteger.Parse(hex, NumberStyles.AllowHexSpecifier);
                generator = 2;                
            }
        }
        private int AssertCorrectSideNumber(string numtext)
        {
            try
            {
                int result = Convert.ToInt32(numtext);
                if (result >= 1) return result;
                else MessageBox.Show("Введите положительное число");
                return 0;
            }
            catch (Exception)
            {
                MessageBox.Show("Указанное число неверно.");
            }
            return 0;

        }
        private void InitializeSimpleView()
        {
            SimpleViewA.IsEnabled = SimpleViewB.IsEnabled = true;
            SimpleViewA.Visibility = SimpleViewB.Visibility = Visibility.Visible;
            SimpleViewResultLabelA.Text = SimpleViewResultLabelB.Text = "Введите секретное значение в поле выше";
            SimpleViewResultLabelA.Foreground = SimpleViewResultLabelB.Foreground = Brushes.Black;
        }
        private void Demo_Click(object sender, RoutedEventArgs e)
        {
            detailed_mode = false;
            ShowcaseSetup();
            InitializeSimpleView();    
        }

        private void ShowcaseSetup()
        {
            // Common setup features
            if (detailed_mode) {
                MessageBoxResult res = MessageBox.Show("В данном режиме в целях упрощения чтения этапов вычисления проводятся в кольце вычетов малого простого числа.\n" +
                     "Если вы хотите продолжить в упрощенном режиме (q = 227), выберите \"Да\"\n" +
                     "Если вы хотите использовать большое q, выберите \"Нет\" (большие числа будут отображаться в виде шестнадцатиричных строк)\n" +
                     "Иначе, выберите \"Отмена\"", "Использовать упрощенную форму?", MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Yes)
                {
                    TurnDetailedViewGenerator(detailed_mode);
                }
                else if (res == MessageBoxResult.No)
                {
                    TurnDetailedViewGenerator(false);
                }
                else
                {
                    return;
                }

            }
            else TurnDetailedViewGenerator(false);
            userA_ready = userB_ready = false;
            ExportLog.IsEnabled = false;
            ExportLog.Visibility = Visibility.Visible;
            GridA.Visibility = GridB.Visibility = Visibility.Visible;
            GridA.IsEnabled = GridB.IsEnabled = true;
            SecretInputA.IsEnabled = SecretInputB.IsEnabled = true; 
            ReadyButtonAText.Text = ReadyButtonBText.Text = "Готов";
            ReadyButtonA.IsEnabled = ReadyButtonB.IsEnabled = true;
            // Simple View off when detailed mode
            SimpleViewA.Visibility = SimpleViewB.Visibility = detailed_mode ? Visibility.Hidden
                                                                            : Visibility.Visible;
            SimpleViewA.IsEnabled = SimpleViewB.IsEnabled = !detailed_mode;

            DetailedViewA.Visibility = DetailedViewB.Visibility = detailed_mode ? Visibility.Visible
                                                                                : Visibility.Hidden;
            DetailedViewA.IsEnabled = DetailedViewB.IsEnabled = detailed_mode;

            DetailedButtonsGrid.Visibility = Visibility.Visible;
            LeftArrow.Visibility =RightArrow.Visibility = detailed_mode ? Visibility.Visible : Visibility.Hidden;
            DetailedButtonsGrid.IsEnabled = true;
        }

        private void HopefullyWorkingShowcase(int x, int y)
        {
            bool smallQ = q == 227;

            Log.Clear();
            if (smallQ) Log.Add(new Stagelog()
            {
                A_Actions = "Начальные условия:\nq = 227\ng = 2\nx = " + x,
                B_Actions = "Начальные условия:\nq = 227\ng = 2\ny = " + y

            });
            else
                Log.Add(new Stagelog()
                {
                    A_Actions = "Начальные условия:\nq = 2^1536 - 2^1472 - 1 + 2^64 * { [2^1406 pi] + 741804 }\ng = 2\nx = " + x,
                    B_Actions = "Начальные условия:\nq = 2^1536 - 2^1472 - 1 + 2^64 * { [2^1406 pi] + 741804 }\ng = 2\ny = " + y
                });
            Random rand = new Random();
            int a1=0, a2=0, a3=0, b1=0, b2=0, b3=0;
            bool RandomValuesSecuredA = false;
            bool RandomValuesSecuredB = false;
            BigInteger g1a1 = 0, g1b1 = 0;

            while (!(RandomValuesSecuredA && RandomValuesSecuredB))
            {
                Stagelog genStage = new Stagelog(){
                    A_Actions = string.Format("Оставляет a1: {0}", a1),
                    B_Actions = string.Format("Оставляет b1: {0}", b1)
                };

                if (!RandomValuesSecuredA) {
                    a1 = smallQ ? rand.Next(227) : rand.Next();
                    genStage.A_Actions = string.Format("Генерирует\na1: {0}", a1);
                }
                if (!RandomValuesSecuredB) {
                    b1 = smallQ ? rand.Next(227) : rand.Next();
                    genStage.B_Actions = string.Format("Генерирует\nb1: {0}", b1);
                }
                Log.Add(genStage);

                // А и B обменивааются g^a1, g^b1
                g1a1 = BigInteger.ModPow(generator, a1, q);
                g1b1 = BigInteger.ModPow(generator, b1, q);

                if (smallQ)
                    Log.Add(new Stagelog()
                    {
                        A_Actions = string.Format("g1 ^ a1 :\n ({0}^{1}) mod {3} = {2}\nA->B:{2}", generator, a1, g1a1, q),
                        B_Actions = string.Format("g1 ^ b1 :\n ({0}^{1}) mod {3} = {2}\nB->A:{2}", generator, b1, g1b1, q)
                    });
                else
                    Log.Add(new Stagelog()
                    {
                        A_Actions = string.Format("g1 ^ a1 : ({0}^{1}) mod q={2}\nA->B:{2}", generator, a1, g1a1.ToString("X")),
                        B_Actions = string.Format("g1 ^ b1 : ({0}^{1}) mod q={2}\nB->A:{2}", generator, b1, g1b1.ToString("X"))
                    });
                Stagelog errStage = new Stagelog(){ A_Actions="Ошибок не выявлено", B_Actions="Ошибок не выявлено"};
                if (g1a1 == 1)
                    errStage.A_Actions = string.Format("Получили значение 1, необходимо генерировать число заново");
                if (g1b1 == 1)
                    errStage.B_Actions = string.Format("Получили значение 1, необходимо генерировать число заново");
                RandomValuesSecuredA = g1a1 != 1;
                RandomValuesSecuredB = g1b1 != 1;
                if (!(RandomValuesSecuredA && RandomValuesSecuredB)) Log.Add(errStage);
            }

            // Обе стороны вычисляют g2 = g1 ^ a2b2
            BigInteger g2_a = BigInteger.ModPow(g1a1, b1, q);
            BigInteger g2_b = BigInteger.ModPow(g1b1, a1, q);
            BigInteger g2 = g2_a;

            if (smallQ)
                Log.Add(new Stagelog()
                {
                    A_Actions = string.Format("g2 = g1^b1 ^ a1 :\n ({0}^{1}) mod {3} = {2}\nЗначения совпадают",
                    g1b1, a1, g2_a, q),
                    B_Actions = string.Format("g2 = g1^a1 ^ b1 :\n ({0}^{1}) mod {3} = {2}\nЗначения совпадают",
                    g1a1, b1, g2_b, q)
                });
            else
                Log.Add(new Stagelog()
                {
                    A_Actions = string.Format("g2 = g1^b1 ^ a1 : ({0}^{1}) mod q={2}\nЗначения совпадают", g1b1.ToString("X"), a1, g2_a.ToString("X")),
                    B_Actions = string.Format("g2 = g1^a1 ^ b1 : ({0}^{1}) mod q={2}\nЗначения совпадают", g1a1.ToString("X"), b1, g2_b.ToString("X"))
                });


            RandomValuesSecuredA = RandomValuesSecuredB = false;
            BigInteger g1a2 = 0, g1b2 = 0;
            while (!(RandomValuesSecuredA && RandomValuesSecuredB))
            {
                Stagelog genStage = new Stagelog()
                {
                    A_Actions = string.Format("Оставляет a2: {0}", a1),
                    B_Actions = string.Format("Оставляет b2: {0}", b1)
                };

                if (!RandomValuesSecuredA)
                {
                    a2 = smallQ ? rand.Next(227) : rand.Next();
                    genStage.A_Actions = string.Format("Генерирует\na2: {0}", a2);
                }
                if (!RandomValuesSecuredB)
                {
                    b2 = smallQ ? rand.Next(227) : rand.Next();
                    genStage.B_Actions = string.Format("Генерирует\nb2: {0}", b2);
                }
                Log.Add(genStage);
                // Повторяется процедура с новыми a2, b2
                g1a2 = BigInteger.ModPow(generator, a2, q);
                g1b2 = BigInteger.ModPow(generator, b2, q);
                if (smallQ)
                    Log.Add(new Stagelog()
                    {
                        A_Actions = string.Format("Повторяем для а2\ng1 ^ a2 :\n ({0}^{1}) mod {3} = {2}\nA->B: {2}",
                        generator, a2, g1a2, q),
                        B_Actions = string.Format("Повторяем для b2\ng1 ^ b2 :\n ({0}^{1}) mod {3} = {2}\nB->A: {2}",
                        generator, b2, g1b2, q)
                    });
                else
                    Log.Add(new Stagelog()
                    {
                        A_Actions = string.Format("Повторяем для а2\ng1 ^ a2 : ({0}^{1}) mod q={2}\nA->B:{2}", generator, a2, g1a2.ToString("X")),
                        B_Actions = string.Format("Повторяем для b2\ng1 ^ b2 : ({0}^{1}) mod q={2}\nB->A:{2}", generator, b2, g1b2.ToString("X"))
                    });
                Stagelog errStage = new Stagelog() { A_Actions = "Ошибок не выявлено", B_Actions = "Ошибок не выявлено" };
                if (g1a2 == 1)
                    errStage.A_Actions = string.Format("Получили значение 1, необходимо генерировать число заново");
                if (g1b2 == 1)
                    errStage.B_Actions = string.Format("Получили значение 1, необходимо генерировать число заново");
                RandomValuesSecuredA = g1a2 != 1;
                RandomValuesSecuredB = g1b2 != 1;
                if (!(RandomValuesSecuredA && RandomValuesSecuredB)) Log.Add(errStage);
            }

            a3 = smallQ ? rand.Next(227) : rand.Next();
            b3 = smallQ ? rand.Next(227) : rand.Next();


           
            
            BigInteger g1b2a2 = BigInteger.ModPow(g1b2, a2,q);
            BigInteger g3 = BigInteger.ModPow(g1a2, b2,q);

            if (smallQ)
                 Log.Add(new Stagelog()
                 {
                     A_Actions = string.Format("g3 = g1^b2 ^ a2 :\n ({0}^{1}) mod {3} = {2}\nЗначения совпадают", 
                     g1b2, a2, g1b2a2, q),
                     B_Actions = string.Format("g3 = g1^a2 ^ b2 :\n ({0}^{1}) mod {3} = {2}\nЗначения совпадают",
                     g1a2, b2, g3, q)
                 });
            else
                Log.Add(new Stagelog()
                {
                    A_Actions = string.Format("g3 = g1^b2 ^ a2 :\n ({0}^{1}) mod q={2}\nЗначения совпадают", g1b2.ToString("X"), a2, g1b2a2.ToString("X")),
                    B_Actions = string.Format("g3 = g1^a2 ^ b2 :\n ({0}^{1}) mod q={2}\nЗначения совпадают", g1a2.ToString("X"), b2, g3.ToString("X"))
                });
            // Упаковка секретов
            BigInteger Pa = BigInteger.ModPow(g3, a3,q);
            BigInteger Pb = BigInteger.ModPow(g3, b3, q);

            if (smallQ)
                Log.Add(new Stagelog()
                {
                    A_Actions = string.Format("Pa = g3 ^ a3 :\n ({0}^{1}) mod {3} = {2}\nA->B: {2}",
                    g3, a3, Pa, q),
                    B_Actions = string.Format("Pb = g3 ^ b3 :\n ({0}^{1}) mod {3} = {2}\nB->A: {2}",
                    g3, b3, Pb, q)
                });
            else
                Log.Add(new Stagelog()
                {
                    A_Actions = string.Format("Pa = g3 ^ a3 : ({0}^{1}) mod q={2}\nA->B:Pa", g3.ToString("X"), a3, Pa.ToString("X")),
                    B_Actions = string.Format("Pb = g3 ^ b3 : ({0}^{1}) mod q={2}\nB->A:Pb", g3.ToString("X"), b3, Pb.ToString("X"))
                });

            BigInteger Qa = BigInteger.Multiply(BigInteger.ModPow(generator, a3, q),
               BigInteger.ModPow(g2, x, q)) % q;
            BigInteger Qb = BigInteger.Multiply(BigInteger.ModPow(generator, b3, q),
                BigInteger.ModPow(g2, y,q)) % q;

            if(smallQ)
                Log.Add(new Stagelog()
                {
                    A_Actions = string.Format("Упаковка секрета\nQa = g1^a3 * g2^x :\n ({0}* ({1}^{2}) mod q) mod {4} = {3}\nA->B: {3}",
                                                        BigInteger.ModPow(generator, a3, q), g2, x, Qa, q),
                    B_Actions = string.Format("Упаковка секрета\nQb = g1^b3 * g2^y :\n ({0}* ({1}^{2}) mod q) mod {4} = {3}\nB->A: {3}",
                                                        BigInteger.ModPow(generator, b3, q), g2, y, Qb,q)
                });
            else
                Log.Add(new Stagelog()
                {
                    A_Actions = string.Format("Упаковка секрета\nQa = g1^a3 * g2^x : ({0}* ({1}^{2}) mod q) mod q={3}\nA->B:Qa", 
                                                        BigInteger.ModPow(generator, a3, q).ToString("X"), g2.ToString("X"),
                    x, Qa.ToString("X")),
                    B_Actions = string.Format("Упаковка секрета\nQb = g1^b3 * g2^y : ({0}* ({1}^{2}) mod q) mod q={3}\nB->A:Qb",
                                                        BigInteger.ModPow(generator, b3, q).ToString("X"), g2.ToString("X"), y, Qb.ToString("X"))
                });
            // --Обмен--

            BigInteger Q = BigInteger.Multiply(Qa, BigInteger.ModPow(Qb, q-2, q)) % q;
            BigInteger Ra = BigInteger.ModPow(Q, a2,q);
            BigInteger Rb = BigInteger.ModPow(Q, b2,q);
            
            if(smallQ)
                Log.Add(new Stagelog()
                {
                    A_Actions = string.Format("Ra = (Qa / Qb)^a3:\n (({0}/{1}) mod q) ^ {2} mod {4} = {3}\nA->B: {3}", 
                    Qa, Qb, a3, Ra, q),
                    B_Actions = string.Format("Rb = (Qa / Qb)^b3:\n (({0}/{1}) mod q) ^ {2} mod {4} = {3}\nB->A: {3}", 
                    Qa, Qb, b3, Rb, q)
                });
            else
                Log.Add(new Stagelog()
                {
                    A_Actions = string.Format("Ra= (Qa / Qb)^a3: (({0}/{1}) mod q) ^ {2} mod q={3}\nA->B: Ra", Qa.ToString("X"), Qb.ToString("X"), a3, Ra.ToString("X")),
                    B_Actions = string.Format("Rb= (Qa / Qb)^b3: (({0}/{1}) mod q) ^ {2} mod q={3}\nB->A: Rb", Qa.ToString("X"), Qb.ToString("X"), b3, Rb.ToString("X"))
                });
            // Общее вычисление
            BigInteger Rab = BigInteger.ModPow(Ra, b2, q);
            BigInteger Rab_b = BigInteger.ModPow(Rb, a2, q);

            if (smallQ)
                Log.Add(new Stagelog()
                {
                    A_Actions = string.Format("Rab = Rb ^ a3 :\n ({0}^{1}) mod {3} = {2}\nЗначения равны",
                    Rb, a3, Rab,q),
                    B_Actions = string.Format("Rab = Ra ^ b3 :\n ({0}^{1}) mod {3} = {2}\nЗначения равны", 
                    Ra, b3, Rab_b, q)
                });
            else
                Log.Add(new Stagelog()
                {
                    A_Actions = string.Format("Rab= Rb ^ a3 : ({0}^{1}) mod q={2}\nЗначения равны", Rb.ToString("X"), a3, Rab.ToString("X")),
                    B_Actions = string.Format("Rab= Ra ^ b3 : ({0}^{1}) mod q={2}\nЗначения равны", Ra.ToString("X"), b3, Rab_b.ToString("X"))
                });

            // Проверка что х=у
            BigInteger div = BigInteger.Multiply(Pa, BigInteger.ModPow(Pb, q - 2, q)) % q;
            if (smallQ)
                Log.Add(new Stagelog()
                {
                    A_Actions = string.Format("Проверка равенства секретов\nRab == (Pa / Pb) :\n {0} == ({1}^{2}) mod {4} = {3}",
                    Rab, Pa, Pb, div, q),
                    B_Actions = string.Format("Проверка равенства секретов\nRab == (Pa / Pb) :\n {0} == ({1}^{2}) mod {4} = {3}",
                    Rab, Pa, Pb, div, q)
                });

            else
                Log.Add(new Stagelog()
                {
                    A_Actions = string.Format("Проверка равенства секретов\nRab == (Pa / Pb): {0} == ({1}^{2}) mod q={3}", Rab.ToString("X"), Pa.ToString("X"), Pb.ToString("X"), div.ToString("X")),
                    B_Actions = string.Format("Проверка равенства секретов\nRab == (Pa / Pb): {0} == ({1}^{2}) mod q={3}", Rab.ToString("X"), Pa.ToString("X"), Pb.ToString("X"), div.ToString("X"))
                });
            bool confirmation = div == Rab;

            if (confirmation)
            {
                SimpleViewResultLabelA.Text = SimpleViewResultLabelB.Text = "Значения равны";
                SimpleViewResultLabelA.Foreground = SimpleViewResultLabelB.Foreground = Brushes.Green;
                Log.Add(new Stagelog()
                {
                    A_Actions = string.Format("Значения равны"),
                    B_Actions = string.Format("Значения равны")
                });
            }
            else
            {
                SimpleViewResultLabelA.Text = SimpleViewResultLabelB.Text = "Значения не равны";
                SimpleViewResultLabelA.Foreground = SimpleViewResultLabelB.Foreground = Brushes.Red;
                Log.Add(new Stagelog()
                {
                    A_Actions = string.Format("Значения не равны"),
                    B_Actions = string.Format("Значения не равны")
                });
            }

        }

       
        private void DetailedDemoButton_Click(object sender, RoutedEventArgs e)
        {
            detailed_mode = true;
            StageLabelA.Visibility = StageLabelB.Visibility = Visibility.Hidden;
            ActionBoxA.Text = ActionBoxB.Text = "Введите секретное значение в поле выше";
            ActionBoxA.Foreground = ActionBoxB.Foreground = Brushes.Black;
            LeftArrow.IsEnabled = RightArrow.IsEnabled = false;
            ShowcaseSetup();
        }
        private void DetailedViewStage(int stageindex)
        {
            StageLabelA.Content = StageLabelB.Content = string.Format("Этап {0}", stageindex);
            Stagelog current = Log[stageindex];
            ActionBoxA.Text = current.A_Actions;
            ActionBoxB.Text = current.B_Actions;
        }
        private void ReadyButtonA_Click(object sender, RoutedEventArgs e)
        {
            A_value = AssertCorrectSideNumber(SecretInputA.Text);
            if (A_value == 0) return;
            SecretInputA.IsEnabled = userA_ready;
            userA_ready = !userA_ready;
            if (userA_ready)
            {
                if (userB_ready)
                {
                    ReadyButtonA.IsEnabled = ReadyButtonB.IsEnabled = false;
                    HopefullyWorkingShowcase(A_value,B_value);
                    ExportLog.IsEnabled = true;
                    if (detailed_mode)
                    {
                        current_stage = 0;
                        StageLabelA.Visibility = StageLabelB.Visibility = Visibility.Visible;
                        DetailedViewStage(current_stage);
                        LeftArrow.IsEnabled = false;
                        RightArrow.IsEnabled = true;
                    }
                }
                else
                {
                    ReadyButtonAText.Text = "Отмена";
                    SimpleViewResultLabelA.Text= "Ожидаем сторону B";
                    ActionBoxA.Text= "Ожидаем сторону B";
                }
            }
            else
            {
                ReadyButtonAText.Text = "Готов";
                SimpleViewResultLabelA.Text = "Введите секретное значение в поле выше";
                ActionBoxA.Text = "Введите секретное значение в поле выше";
            }
        }

        private void ReadyButtonB_Click(object sender, RoutedEventArgs e)
        {
            B_value = AssertCorrectSideNumber(SecretInputB.Text);
            if (B_value == 0) return;
            SecretInputB.IsEnabled = userB_ready;
            userB_ready = !userB_ready;
            if (userB_ready)
            {
                if (userA_ready)
                {
                    ReadyButtonA.IsEnabled = ReadyButtonB.IsEnabled = false;
                    HopefullyWorkingShowcase(A_value, B_value);
                    ExportLog.IsEnabled = true;
                    if (detailed_mode)
                    {
                        current_stage = 0;
                        StageLabelA.Visibility = StageLabelB.Visibility = Visibility.Visible;
                        DetailedViewStage(current_stage);
                        LeftArrow.IsEnabled = false;
                        RightArrow.IsEnabled = true;
                    }
                }
                else
                {
                    ReadyButtonBText.Text = "Отмена";
                    SimpleViewResultLabelB.Text= "Ожидаем сторону A";
                    ActionBoxB.Text= "Ожидаем сторону A";
                } 
            }
            else
            {
                ReadyButtonBText.Text = "Готов";
                SimpleViewResultLabelB.Text = "Введите секретное значение в поле выше";
                ActionBoxB.Text = "Введите секретное значение в поле выше";
            }
        }

        private void LeftArrow_Click(object sender, RoutedEventArgs e)
        {
            if (current_stage == 0) e.Handled = true;
            if (current_stage == Log.Count - 1)
                ActionBoxA.Foreground = ActionBoxB.Foreground = Brushes.Black;
            current_stage--;
            DetailedViewStage(current_stage);
            LeftArrow.IsEnabled = current_stage > 0 ;
            RightArrow.IsEnabled = current_stage < Log.Count-1;
            
        }

        private void RightArrow_Click(object sender, RoutedEventArgs e)
        {
            if (current_stage == Log.Count - 1) e.Handled = true;
            current_stage++;
            DetailedViewStage(current_stage);
            if (current_stage == Log.Count - 1)
            {
                if (ActionBoxA.Text=="Значения равны")
                    ActionBoxA.Foreground = ActionBoxB.Foreground = Brushes.Green;
                else
                    ActionBoxA.Foreground = ActionBoxB.Foreground = Brushes.Red;
            }
            else  ActionBoxA.Foreground = ActionBoxB.Foreground = Brushes.Black;

            LeftArrow.IsEnabled = current_stage > 0;
            RightArrow.IsEnabled = current_stage < Log.Count - 1;
        }

        private void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private void TaskBtn_Click(object sender, RoutedEventArgs e)
        {
            Task task = new Task();
            task.ShowDialog();
        }

        private void ExportLog_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            if (save.ShowDialog()==true)
            {
                using (FileStream fs = File.Open(save.FileName, FileMode.OpenOrCreate))
                {
                    using (StreamWriter sw = new StreamWriter(fs)) {
                        int i = 0;
                        foreach (Stagelog slog in Log)
                        {
                            sw.WriteLine();
                            sw.WriteLine(string.Format("Этап {0}", i));
                            sw.WriteLine(string.Format("Сторона А:\n{0}", slog.A_Actions));
                            sw.WriteLine(string.Format("Сторона B:\n{0}\n", slog.B_Actions));
                            i++;
                        }
                    }
                }
            }
        }
    }
}
