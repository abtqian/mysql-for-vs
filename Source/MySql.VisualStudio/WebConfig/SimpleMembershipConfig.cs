﻿// Copyright © 2009, 2014, Oracle and/or its affiliates. All rights reserved.
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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Security;
using System.Xml;

namespace MySql.Data.VisualStudio.WebConfig
{
  internal struct SimpleMembershipOptions
  {
    public string UserTableName;
    public string UserIdColumn;
    public string UserNameColumn;
    public bool AutoGenerateTables;
  }

  internal class SimpleMembershipConfig : GenericConfig
  {
    private new SimpleMembershipOptions defaults = new SimpleMembershipOptions();
    private new SimpleMembershipOptions values;
    private string _connectorVerInstalled;

    public SimpleMembershipConfig()
      : base()
    {
      typeName = "MySqlSimpleMembershipProvider";
      sectionName = "membership";
    }

    public SimpleMembershipOptions SimpleMemberOptions
    {
      get { return values; }
      set { values = value; }
    }

    public override void Initialize(WebConfig wc)
    {
      base.Initialize(wc);
      //GetDefaults();
      XmlElement e = wc.GetProviderSection(sectionName);
      if (e != null)
      {
        string currentProvider = e.GetAttribute("defaultProvider");
        if (!currentProvider.Equals(typeName, StringComparison.InvariantCultureIgnoreCase))
        {
          base.DefaultProvider = typeName;
          OriginallyEnabled = false;
        }
        else
        {
          base.DefaultProvider = currentProvider;
          OriginallyEnabled = true;
        }
      }

      e = wc.GetProviderElement(sectionName);
      if (e != null)
      {
        if (e.HasAttribute("name"))
        {
          string providerName = e.GetAttribute("name");
          base.values.ProviderName = !OriginallyEnabled ? typeName : providerName;
        }
        if (e.HasAttribute("connectionStringName"))
        {
          string connStrName = e.GetAttribute("connectionStringName");
          base.values.ConnectionStringName = !OriginallyEnabled ? "LocalMySqlServer" : connStrName;
        }
        if (e.HasAttribute("description"))
          base.values.AppDescription = e.GetAttribute("description");
        if (e.HasAttribute("applicationName"))
          base.values.AppName = e.GetAttribute("applicationName");
      }
      base.values.ConnectionString = wc.GetConnectionString(base.values.ConnectionStringName);

      NotInstalled = !_connectorVerInstalled.Contains("6.9");

      Enabled = OriginallyEnabled;

      values = defaults;
      e = wc.GetProviderElement(sectionName);
      if (e == null) return;

      if (e.HasAttribute("userTableName"))
        values.UserTableName = e.GetAttribute("userTableName");
      if (e.HasAttribute("userIdColumn"))
        values.UserIdColumn = e.GetAttribute("userIdColumn");
      if (e.HasAttribute("userNameColumn"))
        values.UserNameColumn = e.GetAttribute("userNameColumn");
      if (e.HasAttribute("autoGenerateTables"))
        values.AutoGenerateTables = Convert.ToBoolean(e.GetAttribute("autoGenerateTables"));
    }

    public override void GetDefaults()
    {
      _connectorVerInstalled = ConnectorVersionInstalled().FirstOrDefault();
      ProviderSettings providerSet = GetMachineSettings();
      if (providerSet != null)
      {
        base.ProviderType = providerSet.Type.Replace("MySQLMembershipProvider", typeName);
      }
      else
      {
        base.ProviderType = string.Format("MySql.Web.Security.MySqlSimpleMembershipProvider,MySql.Web,Version={0},Culture=neutral,PublicKeyToken=c5687fc88969c44d", _connectorVerInstalled);
      }
      base.defaults.ProviderName = typeName;
      base.defaults.ConnectionStringName = "LocalMySqlServer";
      base.defaults.AppName = "/";
      base.defaults.AppDescription = "MySqlSimpleMembership Application";
      base.defaults.AutoGenSchema = true;
      base.defaults.WriteExceptionToLog = false;
      base.defaults.EnableExpireCallback = false;
    }

    protected override void SaveProvider(XmlElement provider)
    {
      base.SaveProvider(provider);

      provider.SetAttribute("userTableName", values.UserTableName.ToString());
      provider.SetAttribute("userIdColumn", values.UserIdColumn.ToString());
      provider.SetAttribute("userNameColumn", values.UserNameColumn.ToString());
      provider.SetAttribute("autoGenerateTables", values.AutoGenerateTables.ToString());
    }

    protected override ProviderSettings GetMachineSettings()
    {
      Configuration machineConfig = ConfigurationManager.OpenMachineConfiguration();
      MembershipSection section = (MembershipSection)machineConfig.SectionGroups["system.web"].Sections[sectionName];
      foreach (ProviderSettings p in section.Providers)
        if (p.Type.Contains("MySQLMembershipProvider")) return p;
      return null;
    }

    internal IEnumerable<string> ConnectorVersionInstalled()
    {
      List<string> mysqlVers = new List<string>();
      try
      {
        string displayName = "";
        foreach (string registryKey in new string[] { @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall", })
        {
          using (Microsoft.Win32.RegistryKey regKey = Registry.LocalMachine.OpenSubKey(registryKey))
          {
            foreach (RegistryKey subKey in regKey.GetSubKeyNames().Select(keyName => regKey.OpenSubKey(keyName)))
            {
              displayName = subKey.GetValue("DisplayName") as string;
              if (!string.IsNullOrEmpty(displayName) && !displayName.StartsWith("{"))
                mysqlVers.Add(displayName);
            }
          }
        }

        using (Microsoft.Win32.RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
        {
          foreach (RegistryKey subKey in regKey.GetSubKeyNames().Select(keyName => regKey.OpenSubKey(keyName)))
          {
            displayName = subKey.GetValue("DisplayName") as string;
            if (!string.IsNullOrEmpty(displayName) && !displayName.StartsWith("{"))
              mysqlVers.Add(displayName);
          }
        }

        return (from regKeyName in mysqlVers
                let connectorVers = regKeyName
                where regKeyName.ToLower().Contains("connector net")
                let vers = connectorVers.Split(' ').Where(ver => ver.Contains("."))
                from ver in vers
                select ver).Distinct();
      }
      catch 
      {     
      }

      return mysqlVers;
      
    }
  }
}