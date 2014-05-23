﻿// Copyright © 2008, 2014, Oracle and/or its affiliates. All rights reserved.
//
// MySQL for Visual Studio is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
// MySQL Connectors. There are special exceptions to the terms and 
// conditions of the GPLv2 as it is applied to this software, see the 
// FLOSS License Exception
// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
//
// This program is free software; you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation; version 2 of the License.
//
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
//
// You should have received a copy of the GNU General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using MySql.Data.VisualStudio.SchemaComparer;
using MySql.Data.VisualStudio.Wizards.WindowsForms;


namespace MySql.Data.VisualStudio.Wizards.WindowsForms
{
  internal class CSharpTypedDatasetMasterDetailStrategy : CSharpMasterDetailStrategy
  {
    internal CSharpTypedDatasetMasterDetailStrategy(StrategyConfig config)
      : base(config)
    {
    }

    protected override void WriteUsingUserCode()
    {
       // Nothing
    }

    protected override void WriteFormLoadCode()
    {
      Writer.WriteLine("string strConn = \"{0};\";", ConnectionString);
      Writer.WriteLine("ad = new MySqlDataAdapter(\"select * from `{0}`\", strConn);", TableName);
      Writer.WriteLine("MySqlCommandBuilder builder = new MySqlCommandBuilder(ad);");
      Writer.WriteLine("ad.Fill(this.newDataSet.{0});", CanonicalTableName );
      Writer.WriteLine("ad.DeleteCommand = builder.GetDeleteCommand();");
      Writer.WriteLine("ad.UpdateCommand = builder.GetUpdateCommand();");
      Writer.WriteLine("ad.InsertCommand = builder.GetInsertCommand();");

      Writer.WriteLine("ad{0} = new MySqlDataAdapter(\"select * from `{1}`\", strConn);", CanonicalDetailTableName, DetailTableName );
      Writer.WriteLine("builder = new MySqlCommandBuilder(ad{0});", CanonicalDetailTableName);
      Writer.WriteLine("ad{0}.Fill(this.newDataSet.{0});", CanonicalDetailTableName );
      Writer.WriteLine("ad{0}.DeleteCommand = builder.GetDeleteCommand();", CanonicalDetailTableName);
      Writer.WriteLine("ad{0}.UpdateCommand = builder.GetUpdateCommand();", CanonicalDetailTableName);
      Writer.WriteLine("ad{0}.InsertCommand = builder.GetInsertCommand();", CanonicalDetailTableName);
      
      RetrieveFkColumns();
      StringBuilder sbSrcCols = new StringBuilder("new DataColumn[] { ");
      StringBuilder sbDstCols = new StringBuilder("new DataColumn[] { ");
      for (int i = 0; i < FkColumnsSource.Count; i++)
      {
        // Both FkColumnsSource & FkColumnsDest have the item count
        sbSrcCols.AppendFormat(" newDataSet.{0}.Columns[ \"{1}\" ] ", CanonicalTableName,
          FkColumnsSource[i]);
        sbDstCols.AppendFormat(" newDataSet.{0}.Columns[ \"{1}\" ] ", CanonicalDetailTableName,
          FkColumnsDest[i]);
      }
      sbSrcCols.Append("}");
      sbDstCols.Append("}");
        
      Writer.WriteLine("newDataSet.Relations.Add( new DataRelation( \"{0}\", {1}, {2} ) );", 
        ConstraintName, sbSrcCols.ToString(), sbDstCols.ToString() );

      Writer.WriteLine("{0}BindingSource.DataSource = {1}BindingSource;", CanonicalDetailTableName, CanonicalTableName);
      Writer.WriteLine("{0}BindingSource.DataMember = \"{1}\";", CanonicalDetailTableName, ConstraintName);
      Writer.WriteLine("dataGridView1.DataSource = {0}BindingSource;", CanonicalDetailTableName);
    }

    private void WriteValidationCodeIndividiualControl()
    {
      List<ColumnValidation> validationColumns = ValidationColumns;
      for (int i = 0; i < validationColumns.Count; i++)
      {
        ColumnValidation cv = validationColumns[i];
        string idColumnCanonical = GetCanonicalIdentifier(cv.Column.ColumnName);
        Writer.WriteLine("private void {0}TextBox_Validating(object sender, CancelEventArgs e)", idColumnCanonical);
        Writer.WriteLine("{");
        Writer.WriteLine("  e.Cancel = false;");
        if (cv.Required)
        {
          Writer.WriteLine("  if( string.IsNullOrEmpty( {0}TextBox.Text ) ) {{ ", idColumnCanonical);
          Writer.WriteLine("    e.Cancel = true;");
          Writer.WriteLine("    errorProvider1.SetError( {0}TextBox, \"The field {1} is required\" ); ", idColumnCanonical, cv.Name);
          Writer.WriteLine("  }");
        }
        if (cv.IsNumericType())
        {
          Writer.WriteLine("  int v;");
          Writer.WriteLine("  string s = {0}TextBox.Text;", idColumnCanonical);
          Writer.WriteLine("  if( !int.TryParse( s, out v ) ) {");
          Writer.WriteLine("    e.Cancel = true;");
          Writer.WriteLine("    errorProvider1.SetError( {0}TextBox, \"The field {1} must be numeric.\" );", idColumnCanonical, cv.Name);
          Writer.WriteLine("  }");
          if (cv.MinValue != null)
          {
            Writer.WriteLine(" else if( cv.MinValue > v ) { ");
            Writer.WriteLine("   e.Cancel = true;");
            Writer.WriteLine("   errorProvider1.SetError( {0}TextBox, \"The field {1} must be greater or equal than {2}.\" );", idColumnCanonical, cv.Name, cv.MinValue);
            Writer.WriteLine(" } ");
          }
          if (cv.MaxValue != null)
          {
            Writer.WriteLine(" else if( cv.MaxValue < v ) { ");
            Writer.WriteLine("   e.Cancel = true;");
            Writer.WriteLine("   errorProvider1.SetError( {0}TextBox, \"The field {1} must be lesser or equal than {2}\" );", idColumnCanonical, cv.Name, cv.MaxValue);
            Writer.WriteLine(" } ");
          }
        }
        Writer.WriteLine("  if( !e.Cancel ) {{ errorProvider1.SetError( {0}TextBox, \"\" ); }} ", idColumnCanonical);
        Writer.WriteLine("}");
        Writer.WriteLine();
      }
    }

    private void WriteValidationCodeDetailsGrid()
    {
      List<ColumnValidation> validationColumns = ValidationColumns;
      Writer.WriteLine("private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)");
      Writer.WriteLine("{");

      Writer.WriteLine("  int v;");
      Writer.WriteLine("  string s;");
      Writer.WriteLine("  DataGridViewRow row = dataGridView1.Rows[e.RowIndex];");
      Writer.WriteLine("  object value = e.FormattedValue;");
      Writer.WriteLine("  e.Cancel = false;");
      Writer.WriteLine("  row.ErrorText = \"\";");
      Writer.WriteLine("  if (row.IsNewRow) return;");
      for (int i = 0; i < validationColumns.Count; i++)
      {
        ColumnValidation cv = validationColumns[i];
        string idColumnCanonical = GetCanonicalIdentifier(cv.Column.ColumnName);

        Writer.WriteLine("  if (e.ColumnIndex == {0})", i);
        Writer.WriteLine("  {");

        if (cv.Required)
        {
          Writer.WriteLine("  if( (value is DBNull) || string.IsNullOrEmpty( value.ToString() ) ) { ");
          Writer.WriteLine("    e.Cancel = true;");
          Writer.WriteLine("    row.ErrorText = \"The field {0} is required\";", cv.Name);
          Writer.WriteLine("    return;");
          Writer.WriteLine("  }");
        }
        if (cv.IsNumericType())
        {
          Writer.WriteLine("  s = value.ToString();");
          Writer.WriteLine("  if( !int.TryParse( s, out v ) ) {");
          Writer.WriteLine("    e.Cancel = true;");
          Writer.WriteLine("    row.ErrorText = \"The field {0} must be numeric.\";", cv.Name);
          Writer.WriteLine("    return;");
          Writer.WriteLine("  }");
          if (cv.MinValue != null)
          {
            Writer.WriteLine(" else if( cv.MinValue > v ) { ");
            Writer.WriteLine("    e.Cancel = true;");
            Writer.WriteLine("    row.ErrorText = \"The field {0} must be greater or equal than {1}.\";", cv.Name, cv.MinValue);
            Writer.WriteLine("    return;");
            Writer.WriteLine(" } ");
          }
          if (cv.MaxValue != null)
          {
            Writer.WriteLine(" else if( cv.MaxValue < v ) { ");
            Writer.WriteLine("    e.Cancel = true;");
            Writer.WriteLine("    row.ErrorText = \"The field {0} must be lesser or equal than {1}\";", cv.Name, cv.MaxValue);
            Writer.WriteLine("    return;");
            Writer.WriteLine(" } ");
          }
        }

        Writer.WriteLine("  }");
      }
      Writer.WriteLine("}");
    }

    protected override void WriteValidationCode()
    {
      bool validationsEnabled = ValidationsEnabled;
      if (validationsEnabled)
      {
        WriteValidationCodeIndividiualControl();
        WriteValidationCodeDetailsGrid();
      }
    }

    protected override void WriteVariablesUserCode()
    {
      Writer.WriteLine("private MySqlDataAdapter ad;");
      Writer.WriteLine("private MySqlDataAdapter ad{0};", CanonicalDetailTableName );
    }

    protected override void WriteSaveEventCode()
    {
      Writer.WriteLine("{0}BindingSource.EndEdit();", CanonicalTableName );
      Writer.WriteLine("{0}BindingSource.EndEdit();", CanonicalDetailTableName );
      Writer.WriteLine("ad.Update(this.newDataSet.{0});", CanonicalTableName );
      Writer.WriteLine("ad{0}.Update(this.newDataSet.{0});", CanonicalDetailTableName);
    }

    protected override void WriteDesignerControlDeclCode()
    {
      Writer.WriteLine("private NewDataSet newDataSet;");
      Writer.WriteLine("private System.Windows.Forms.BindingSource {0}BindingSource;", CanonicalTableName);
      foreach (KeyValuePair<string, Column> kvp in Columns)
      {
        string idColumnCanonical = GetCanonicalIdentifier(kvp.Key);
        Writer.WriteLine("private System.Windows.Forms.TextBox {0}TextBox;", idColumnCanonical);
        Writer.WriteLine("private System.Windows.Forms.Label {0}Label;", idColumnCanonical);
      }
      Writer.WriteLine("private System.Windows.Forms.BindingSource {0}BindingSource;", CanonicalDetailTableName);
      Writer.WriteLine("private System.Windows.Forms.DataGridView dataGridView1;");
    }

    protected override void WriteDesignerControlInitCode()
    {
      Writer.WriteLine("this.bindingNavigator1.BindingSource = this.{0}BindingSource;", CanonicalTableName);
      Writer.WriteLine("// ");
      Writer.WriteLine("// newDataSet");
      Writer.WriteLine("// ");
      Writer.WriteLine("this.newDataSet.DataSetName = \"NewDataSet\";");
      Writer.WriteLine("this.newDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;");

      Writer.WriteLine("// ");
      Writer.WriteLine("// tableBindingSource");
      Writer.WriteLine("// ");
      Writer.WriteLine("this.{0}BindingSource.DataMember = \"{0}\";", CanonicalTableName);
      Writer.WriteLine("this.{0}BindingSource.DataSource = this.newDataSet;", CanonicalTableName);

      WriteControlInitialization(true);
      // DataGrid
      Writer.WriteLine("this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;");
      Writer.WriteLine("this.dataGridView1.Location = new System.Drawing.Point(280, 95);");
      Writer.WriteLine("this.dataGridView1.Name = \"dataGridView1\"; ");
      Writer.WriteLine("this.dataGridView1.Size = new System.Drawing.Size(339, 261);");
      Writer.WriteLine("this.dataGridView1.TabIndex = 0;");
      if (ValidationsEnabled)
      {
        Writer.WriteLine("this.dataGridView1.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dataGridView1_CellValidating);");
      }
      Writer.WriteLine("this.Panel1.Controls.Add(this.dataGridView1);");
    }

    protected override void WriteDesignerBeforeSuspendCode()
    {
      Writer.WriteLine("this.newDataSet = new NewDataSet();");
      Writer.WriteLine("this.dataGridView1 = new System.Windows.Forms.DataGridView();");
      Writer.WriteLine("this.{0}BindingSource = new System.Windows.Forms.BindingSource(this.components);", CanonicalTableName);
      Writer.WriteLine("this.{0}BindingSource = new System.Windows.Forms.BindingSource(this.components);", CanonicalDetailTableName);
    }

    protected override void WriteDesignerAfterSuspendCode()
    {
      Writer.WriteLine("((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();");
      Writer.WriteLine("((System.ComponentModel.ISupportInitialize)(this.{0}BindingSource)).BeginInit();", CanonicalTableName);
      Writer.WriteLine("((System.ComponentModel.ISupportInitialize)(this.{0}BindingSource)).BeginInit();", CanonicalDetailTableName);
    }

    protected override void WriteBeforeResumeSuspendCode()
    {
      Writer.WriteLine("this.Size = new System.Drawing.Size(682, 590);");
      Writer.WriteLine("((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();");
      Writer.WriteLine("((System.ComponentModel.ISupportInitialize)(this.{0}BindingSource)).EndInit();", CanonicalDetailTableName);
      Writer.WriteLine("((System.ComponentModel.ISupportInitialize)(this.{0}BindingSource)).EndInit();", CanonicalTableName);
    }
  }
}