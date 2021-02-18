﻿using System;
using NLog;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsTrucks
{
    public partial class FormParking : Form
    {
        private readonly ParkingCollection parkingCollection;
        private readonly Logger logger;
        public FormParking()
        {
            InitializeComponent();
            parkingCollection = new ParkingCollection(pictureBoxParking.Width, pictureBoxParking.Height);
            logger = LogManager.GetCurrentClassLogger();
        }
        private void ReloadLevels()
        {
            int index = listBoxParking.SelectedIndex;
            listBoxParking.Items.Clear();
            for (int i = 0; i < parkingCollection.Keys.Count; i++)
            {
                listBoxParking.Items.Add(parkingCollection.Keys[i]);
            }
            if (listBoxParking.Items.Count > 0 && (index == -1 || index >= listBoxParking.Items.Count))
            {
                listBoxParking.SelectedIndex = 0;
            }
            else if (listBoxParking.Items.Count > 0 && index > -1 && index < listBoxParking.Items.Count)
            {
                listBoxParking.SelectedIndex = index;
            }
        }
        private void Draw()
        {
            if (listBoxParking.SelectedIndex > -1)
            {
                Bitmap bmp = new Bitmap(pictureBoxParking.Width,pictureBoxParking.Height);
                Graphics gr = Graphics.FromImage(bmp);
                parkingCollection[listBoxParking.SelectedItem.ToString()].Draw(gr);
                pictureBoxParking.Image = bmp;
            }
        }
        private void Add_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxName.Text))
            {
                MessageBox.Show("Введите название парковки", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            logger.Info($"Добавили парковку {textBoxName.Text}");
            parkingCollection.AddParking(textBoxName.Text);
            ReloadLevels();
        }
        private void Delete_Click(object sender, EventArgs e)
        {
            if (listBoxParking.SelectedIndex > -1)
            {
                if (MessageBox.Show($"Удалить парковку {listBoxParking.SelectedItem.ToString()}?", "Удаление", MessageBoxButtons.YesNo,MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    logger.Info($"Удалили парковку{ listBoxParking.SelectedItem.ToString()}");
                    parkingCollection.DelParking(textBoxName.Text);
                    ReloadLevels();
                }
            }
        }
        private void Take_Click(object sender, EventArgs e)
        {
            if (listBoxParking.SelectedIndex > -1 && maskedTextBoxParking.Text != "")
            {
                try
                {
                    var truck = parkingCollection[listBoxParking.SelectedItem.ToString()] - Convert.ToInt32(maskedTextBoxParking.Text);
                    if (truck != null)
                    {
                        {
                            FormTruck form = new FormTruck();
                            form.SetTruck(truck);
                            form.ShowDialog();
                        }
                        logger.Info($"Изъят автомобиль {truck} с места{ maskedTextBoxParking.Text}");
                        Draw();
                    }
                }
                catch (ParkingNotFoundException ex)
                {
                    MessageBox.Show(ex.Message, "Не найдено", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Неизвестная ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void listBoxParking_SelectedIndexChanged(object sender, EventArgs e)
        {
            logger.Info($"Перешли на парковку{ listBoxParking.SelectedItem.ToString()}");
            Draw();
        }
        private void Set_Truck_Click(object sender, EventArgs e)
        {
            var formtruckconfig = new FormTruckConfig();
            formtruckconfig.AddEvent(AddTruck);
            formtruckconfig.Show();
        }
        private void AddTruck(Vehicle Truck)
        {
            if (Truck != null && listBoxParking.SelectedIndex > -1)
            {
                try 
                {
                    if ((parkingCollection[listBoxParking.SelectedItem.ToString()]) + Truck)
                    {
                        Draw();
                        logger.Info($"Добавлен автомобиль {Truck}");
                    }
                    else
                    {
                        MessageBox.Show("Машину не удалось поставить");
                    }
                    Draw();
                }
                catch (ParkingOverflowException ex)
                {
                    MessageBox.Show(ex.Message, "Переполнение", MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Неизвестная ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    parkingCollection.SaveData(saveFileDialog.FileName);
                    MessageBox.Show("Сохранение прошло успешно", "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    logger.Info("Сохранено в файл " + saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Неизвестная ошибка при сохранении", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    parkingCollection.LoadData(openFileDialog.FileName);
                    MessageBox.Show("Загрузили", "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    logger.Info("Загружено из файла " + openFileDialog.FileName);
                    ReloadLevels();
                    Draw();
                }
                catch (ParkingOccupiedPlaceException ex)
                {
                    MessageBox.Show(ex.Message, "Занятое место", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Неизвестная ошибка при сохранении", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
