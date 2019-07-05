using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace CustomerMaintenance
{
    public partial class frmAddModifyCustomer : Form
    {
        public frmAddModifyCustomer()
        {
            InitializeComponent();
            mmaBooks = new MMABooksEntities();
        }

        private MMABooksEntities mmaBooks;
        public bool addCustomer;
        public Customer customer;

        private void frmAddModifyCustomer_Load(object sender, EventArgs e)
        {
            this.LoadComboBox();
            if (addCustomer)
            {
                this.Text = "Add Customer";
                cboStates.SelectedIndex = -1;
            }
            else
            {
                this.Text = "Modify Customer";
                this.DisplayCustomerData();
            }
        }

        private void LoadComboBox()
        {
            try
            {
                // Code a query to retrieve the required information from
                // the States table, and sort the results by state name.
                var states = from s in mmaBooks.States
                             orderby s.StateName
                             select s;

                // Bind the State combo box to the query results.
                foreach (var s in states)
                {
                    cboStates.DataSource = states.ToList();
                    cboStates.DisplayMember = "StateName";
                    cboStates.ValueMember = "StateCode";
                    cboStates.SelectedValue = "StateCode";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }

        private void DisplayCustomerData()
        {
            txtName.Text = customer.Name;
            txtAddress.Text = customer.Address;
            txtCity.Text = customer.City;
            cboStates.SelectedValue = customer.StateCode;
            txtZipCode.Text = customer.ZipCode;
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            if (IsValidData())
            {
                if (addCustomer)
                {
                    customer = new Customer();
                    this.PutCustomerData(customer);
                    
                    // Add the new vendor to the collection of vendors.
                    
                    try
                    {
                        customer.State = (from s in mmaBooks.States
                                          where customer.StateCode == s.StateCode
                                          select s).Single();

                        customer.CustomerID = (from c in mmaBooks.Customers
                                              orderby c.CustomerID descending
                                              select c.CustomerID).First() + 1;

                        mmaBooks.Customers.Add(customer);
                        Console.WriteLine(mmaBooks.SaveChanges());

                        this.DialogResult = DialogResult.OK;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, ex.GetType().ToString());
                    }
                }
                else
                {
                    this.PutCustomerData(customer);
                    try
                    {

                        // Update the database.
                        var query = (from c in mmaBooks.Customers
                                     where c.CustomerID == customer.CustomerID
                                     select c).Single();

                        PutCustomerData(query);

                        mmaBooks.SaveChanges();

                        this.DialogResult = DialogResult.OK;
                    }
                    
                    // Add concurrency error handling.
                    // Place the catch block before the one for a generic exception.

                    catch (Exception ex)
                    {
                        //if state == detached then deleted, if not then changed
                        DbUpdateConcurrencyException(ex as DbUpdateConcurrencyException);
                        MessageBox.Show(ex.Message, ex.GetType().ToString());
                    }
                }
            }
        }

        private bool IsValidData()
        {
             return Validator.IsPresent(txtName) &&
                    Validator.IsPresent(txtAddress) &&
                    Validator.IsPresent(txtCity) &&
                    Validator.IsPresent(cboStates) &&
                    Validator.IsPresent(txtZipCode) &&
                    Validator.IsInt32(txtZipCode);
        }

        private void PutCustomerData(Customer customer)
        {
            customer.Name = txtName.Text;
            customer.Address = txtAddress.Text;
            customer.City = txtCity.Text;
            customer.StateCode = cboStates.SelectedValue.ToString();
            customer.ZipCode = txtZipCode.Text;
        }

        private void cboStates_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void DbUpdateConcurrencyException(DbUpdateConcurrencyException ex)
        {
            if (ex != null)
                ex.Entries.Single().Reload();
        }
    }
}
