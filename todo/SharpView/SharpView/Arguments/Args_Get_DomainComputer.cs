﻿using SharpView.Enums;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SharpView.Arguments
{
    public class Args_Get_DomainComputer
    {
        public string[] Identity { get; set; }
        public string[] SamAccountName { get { return Identity; } set { Identity = value; } }
        
        public bool Unconstrained { get; set; }

        public bool TrustedToAuth { get; set; }

        public bool Printers { get; set; }

        public string SPN { get; set; }
        public string ServicePrincipalName { get { return SPN; } set { SPN = value; } }

        public string OperatingSystem { get; set; }

        public string ServicePack { get; set; }

        public string SiteName { get; set; }

        public bool Ping { get; set; }

        public string Domain { get; set; }

        public string LDAPFilter { get; set; }
        public string Filter { get { return LDAPFilter; } set { LDAPFilter = value; } }

        public string[] Properties { get; set; }

        public string SearchBase { get; set; }
        public string ADSPath { get { return SearchBase; } set { SearchBase = value; } }

        public string Server { get; set; }
        public string DomainController { get { return Server; } set { Server = value; } }

        public SearchScope SearchScope { get; set; } = SearchScope.Subtree;

        private int _ResultPageSize = 200;
        public int ResultPageSize
        {
            get { return _ResultPageSize; }
            set
            {
                if (value < 1 || value > 10000) throw new ArgumentOutOfRangeException("ResultPageSize");
                _ResultPageSize = value;
            }
        }

        private int? _ServerTimeLimit;
        public int? ServerTimeLimit
        {
            get { return _ServerTimeLimit; }
            set
            {
                if (value < 1 || value > 10000) throw new ArgumentOutOfRangeException("ServerTimeLimit");
                _ServerTimeLimit = value;
            }
        }

        public SecurityMasks? SecurityMasks { get; set; }

        public bool Tombstone { get; set; }

        public bool FindOne { get; set; }
        public bool ReturnOne { get { return FindOne; } set { FindOne = value; } }

        public NetworkCredential Credential { get; set; }

        public bool Raw { get; set; }

        public UACEnum UACFilter { get; set; }

        public Args_Get_DomainComputer()
        {

        }

        public Args_Get_DomainComputer(Args_Get_DomainSearcher args)
        {
            Domain = args.Domain;
            LDAPFilter = args.LDAPFilter;
            Properties = args.Properties;
            SearchBase = args.SearchBase;
            Server = args.Server;
            SearchScope = args.SearchScope;
            ResultPageSize = args.ResultPageSize;
            ServerTimeLimit = args.ServerTimeLimit;
            SecurityMasks = args.SecurityMasks;
            Tombstone = args.Tombstone;
            Credential = args.Credential;
        }

        public Args_Get_DomainComputer(Args_Get_DomainObject args)
        {
            Identity = args.Identity;
            Domain = args.Domain;
            LDAPFilter = args.LDAPFilter;
            Properties = args.Properties;
            SearchBase = args.SearchBase;
            Server = args.Server;
            SearchScope = args.SearchScope;
            ResultPageSize = args.ResultPageSize;
            ServerTimeLimit = args.ServerTimeLimit;
            SecurityMasks = args.SecurityMasks;
            Tombstone = args.Tombstone;
            FindOne = args.FindOne;
            Credential = args.Credential;
            UACFilter = args.UACFilter;
            Raw = args.Raw;
        }
    }
}
