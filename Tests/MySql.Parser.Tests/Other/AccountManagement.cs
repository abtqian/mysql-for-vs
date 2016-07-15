﻿// Copyright © 2013, 2016, Oracle and/or its affiliates. All rights reserved.
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
using System.Text;
using Xunit;

namespace MySql.Parser.Tests.Other
{

  public class AccountManagement
  {
    [Fact]
    public void CreateUser1()
    {
      string sql = @"CREATE USER 'jeffrey'@'localhost' IDENTIFIED BY 'mypass';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void CreateUser2()
    {
      string sql = @"CREATE USER 'jeffrey'@'localhost';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void CreateUser3()
    {
      string sql = @"CREATE USER 'jeffrey'@'localhost' IDENTIFIED BY PASSWORD '*90E462C37378CED12064BB3388827D2BA3A9B689';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void CreateUser4()
    {
      string sql = @"CREATE USER 'jeffrey'@'localhost'
IDENTIFIED BY PASSWORD '*90E462C37378CED12064BB3388827D2BA3A9B689', 'me'@'localhost'
IDENTIFIED BY PASSWORD '*90E462C37378CED12064BB3388827D2BA3A9B689';";
      Utility.ParseSql(sql, false);
    }

    public void CreateUser_IfNotExists_5_6()
    {
      string sql = @"CREATE USER IF NOT EXISTS 'jeffrey'@'localhost';";
      Utility.ParseSql(sql, true, new Version(5, 6));
    }

    [Fact]
    public void CreateUser_IfNotExists_5_7()
    {
      string sql = @"CREATE USER IF NOT EXISTS 'jeffrey'@'localhost';";
      Utility.ParseSql(sql, false, new Version(5, 7, 6));
    }

    [Fact]
    public void DropUser()
    {
      string sql = @"DROP USER 'jeffrey'@'localhost';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void DropUser2()
    {
      string sql = @"DROP USER 'jeffrey'@'localhost', 'me'@'localhost';";
      Utility.ParseSql(sql, false);
    }

    public void DropUser_IfExists_5_6()
    {
      string sql = @"DROP USER IF EXISTS 'jeffrey'@'localhost';";
      Utility.ParseSql(sql, true, new Version(5, 6));
    }

    [Fact]
    public void DropUser_IfExists_5_7()
    {
      string sql = @"DROP USER IF EXISTS 'jeffrey'@'localhost';";
      Utility.ParseSql(sql, false, new Version(5, 7, 6));
    }

    [Fact]
    public void Grant()
    {
      string sql = @"GRANT ALL ON db1.* TO 'jeffrey'@'localhost';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant2()
    {
      string sql = @"GRANT SELECT ON db2.invoice TO 'jeffrey'@'localhost';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant3()
    {
      string sql = @"GRANT USAGE ON *.* TO 'jeffrey'@'localhost' WITH MAX_QUERIES_PER_HOUR 90;";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant4()
    {
      string sql = @"GRANT ALL ON *.* TO 'someuser'@'somehost';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant5()
    {
      string sql = @"GRANT SELECT, INSERT ON *.* TO 'someuser'@'somehost';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant6()
    {
      string sql = @"GRANT ALL ON mydb.* TO 'someuser'@'somehost';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant7()
    {
      string sql = @"GRANT SELECT, INSERT ON mydb.* TO 'someuser'@'somehost';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant8()
    {
      string sql = @"GRANT SELECT (col1), INSERT (col1,col2) ON mydb.mytbl TO 'someuser'@'somehost';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant9()
    {
      string sql = @"GRANT CREATE ROUTINE ON mydb.* TO 'someuser'@'somehost';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant10()
    {
      string sql = @"GRANT EXECUTE ON PROCEDURE mydb.myproc TO 'someuser'@'somehost';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant11()
    {
      string sql = @"GRANT ALL ON test.* TO ''@'localhost'";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant12()
    {
      string sql = @"GRANT USAGE ON *.* TO ''@'localhost' WITH MAX_QUERIES_PER_HOUR 500 MAX_UPDATES_PER_HOUR 100;";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant13()
    {
      string sql = @"GRANT ALL PRIVILEGES ON test.* TO 'root'@'localhost' IDENTIFIED BY 'goodsecret' REQUIRE SSL;";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant14()
    {
      string sql = @"GRANT ALL PRIVILEGES ON test.* TO 'root'@'localhost'
  IDENTIFIED BY 'goodsecret' REQUIRE X509;";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant15()
    {
      string sql = @"GRANT ALL PRIVILEGES ON test.* TO 'root'@'localhost'
  IDENTIFIED BY 'goodsecret'
  REQUIRE ISSUER '/C=FI/ST=Some-State/L=Helsinki/
    O=MySQL Finland AB/CN=Tonu Samuel/emailAddress=tonu@example.com';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant16()
    {
      string sql = @"GRANT ALL PRIVILEGES ON test.* TO 'root'@'localhost'
  IDENTIFIED BY 'goodsecret'
  REQUIRE SUBJECT '/C=EE/ST=Some-State/L=Tallinn/
    O=MySQL demo client certificate/
    CN=Tonu Samuel/emailAddress=tonu@example.com';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant17()
    {
      string sql = @"GRANT ALL PRIVILEGES ON test.* TO 'root'@'localhost'
  IDENTIFIED BY 'goodsecret'
  REQUIRE CIPHER 'EDH-RSA-DES-CBC3-SHA';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant18()
    {
      string sql = @"GRANT ALL PRIVILEGES ON test.* TO 'root'@'localhost'
  IDENTIFIED BY 'goodsecret'
  REQUIRE SUBJECT '/C=EE/ST=Some-State/L=Tallinn/O=MySQL demo client certificate/
    CN=Tonu Samuel/emailAddress=tonu@example.com'
  AND ISSUER '/C=FI/ST=Some-State/L=Helsinki/O=MySQL Finland AB/CN=Tonu Samuel/emailAddress=tonu@example.com'
  AND CIPHER 'EDH-RSA-DES-CBC3-SHA';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant19()
    {
      string sql = @"GRANT REPLICATION CLIENT ON *.* TO 'user'@'10.10.10.%'";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Grant20()
    {
      string sql = @"GRANT USAGE ON *.* TO 'bob'@'%.loc.gov' IDENTIFIED BY 'newpass';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void GrantProxy51()
    {
      string sql = @"GRANT PROXY ON 'localuser'@'localhost' TO 'externaluser'@'somehost';";
      string result = Utility.ParseSql(sql, true, new Version(5, 1, 0));
      Assert.True(result.IndexOf("'grant'", StringComparison.InvariantCultureIgnoreCase) != -1);
    }

    [Fact]
    public void GrantProxy55()
    {
      string sql = @"GRANT PROXY ON 'localuser'@'localhost' TO 'externaluser'@'somehost';";
      Utility.ParseSql(sql, false, new Version(5, 5, 50));
    }

    [Fact]
    public void Rename()
    {
      string sql = "RENAME USER 'jeffrey'@'localhost' TO 'jeff'@'127.0.0.1';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Revoke()
    {
      string sql = "REVOKE INSERT ON *.* FROM 'jeffrey'@'localhost';";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void Revoke2()
    {
      string sql = "REVOKE ALL PRIVILEGES, GRANT OPTION FROM 'jeffrey'@'localhost', 'jeff'@'127.0.0.1', 'me'@'localhost'";
      Utility.ParseSql(sql, false);
    }

    [Fact]
    public void RevokeProxy51()
    {
      string sql = "REVOKE PROXY ON 'jeffrey'@'localhost' FROM 'jeff'@'127.0.0.1', 'me'@'localhost'";
      string result = Utility.ParseSql(sql, true, new Version(5, 1, 0));
      Assert.True(result.IndexOf("'proxy'", StringComparison.InvariantCultureIgnoreCase) != -1);
    }

    [Fact]
    public void RevokeProxy55()
    {
      string sql = "REVOKE PROXY ON 'jeffrey'@'localhost' FROM 'jeff'@'127.0.0.1', 'me'@'localhost'";
      Utility.ParseSql(sql, false, new Version(5, 5, 50));
    }

    [Fact]
    public void SetPassword()
    {
      string sql = "SET PASSWORD FOR 'bob'@'%.loc.gov' = PASSWORD('newpass');";
      Utility.ParseSql(sql, false);
    }
  }
}
