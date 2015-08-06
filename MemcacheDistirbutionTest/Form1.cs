using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enyim;

namespace MemcacheDistirbutionTest
{

    public partial class Form1 : Form
    {


        private List<IPEndPoint> _nodes;
        private Dictionary<uint, IPEndPoint> _defaultLocatorServers;
        private uint[] _defaulNodekeys;

        private Dictionary<IPEndPoint, KeyCount> _defaultLocatorKeyDistirbution;

        private const int DefaultServerAddressMutations = 100;

        private const int DefaultPortNumber = 1121;

        private List<string> _ipAddressList;

        private Guid dcpId;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dcpId = Guid.NewGuid();

            Initialise();

            Debug.WriteLine("Initialised all the nodes");


        }

        private void Initialise()
        {
            _defaultLocatorServers = new Dictionary<uint, IPEndPoint>();

            _defaulNodekeys = new uint[] { };

            _ipAddressList = new List<string>();

            InitialiseIpAddressList();
            _nodes = new List<IPEndPoint>();

            _defaultLocatorKeyDistirbution = new Dictionary<IPEndPoint, KeyCount>();

            foreach (string ipAddress in _ipAddressList)
            {
                _nodes.Add(ResolveToEndPoint(ipAddress, DefaultPortNumber));
                _defaultLocatorKeyDistirbution.Add(ResolveToEndPoint(ipAddress, DefaultPortNumber), new KeyCount());
            }
        }

        private void InitialiseIpAddressList()
        {
            if (_ipAddressList == null)
            {
                _ipAddressList = new List<string>();
            }

            _ipAddressList.Add("27.22.137.133");
            _ipAddressList.Add("1.156.225.133");
            _ipAddressList.Add("52.57.155.157");
            _ipAddressList.Add("142.205.227.100");
            _ipAddressList.Add("238.94.154.6");
            _ipAddressList.Add("23.45.79.227");
            _ipAddressList.Add("3.100.17.59");


        }


        private IPEndPoint ResolveToEndPoint(string host, int port)
        {


            if (String.IsNullOrEmpty(host))
                throw new ArgumentNullException("host");

            IPAddress address;

            // parse as an IP address
            if (!IPAddress.TryParse(host, out address))
            {
                // not an ip, resolve from dns
                // TODO we need to find a way to specify whihc ip should be used when the host has several
                var entry = System.Net.Dns.GetHostEntry(host);
                address = entry.AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                if (address == null)
                    throw new ArgumentException(String.Format("Could not resolve host '{0}'.", host));
            }

            return new IPEndPoint(address, port);
        }

        private void btnDefaultNodeLocatorDefaultHasing_Click(object sender, EventArgs e)
        {
            Initialise();
            DefaultNodeLocatorDefaultHasingIndex();

            Debug.WriteLine("Finished building index");

            foreach (IPEndPoint key in _nodes)
            {
                if (!_defaultLocatorKeyDistirbution.ContainsKey(key))
                {
                    _defaultLocatorKeyDistirbution.Add(key, new KeyCount());
                }
                else
                {
                    _defaultLocatorKeyDistirbution[key] = new KeyCount();
                }

            }

            Debug.WriteLine("Finished count reset");

            Debug.WriteLine("Starting test for 10000 random rawproductId key distirbution");

            for (int i = 0; i < 10000; i++)
            {
                string key = CreateKey(Guid.NewGuid());

                //Default Key
                IPEndPoint defaultNodeEndPoint = FindNodeDefault(key);

                KeyCount currentKeyCount = _defaultLocatorKeyDistirbution[defaultNodeEndPoint];
                currentKeyCount.Default = currentKeyCount.Default + 1;
                _defaultLocatorKeyDistirbution[defaultNodeEndPoint] = currentKeyCount;

                //base64key

                string base64Key = Convert.ToBase64String(Encoding.UTF8.GetBytes(key), Base64FormattingOptions.None);
                IPEndPoint base64EndPoint = FindNodeDefault(base64Key);

                KeyCount base64KeyCount = _defaultLocatorKeyDistirbution[base64EndPoint];
                base64KeyCount.Base64 = base64KeyCount.Base64 + 1;
                _defaultLocatorKeyDistirbution[base64EndPoint] = base64KeyCount;

                //sha1
                SHA1Managed sh = new SHA1Managed();
                byte[] data = sh.ComputeHash(Encoding.Unicode.GetBytes(key));

                IPEndPoint sha1EndPoint = FindNodeDefault(Convert.ToBase64String(data, Base64FormattingOptions.None));

                KeyCount sha1KeyCount = _defaultLocatorKeyDistirbution[sha1EndPoint];
                sha1KeyCount.Sha1 = sha1KeyCount.Sha1 + 1;
                _defaultLocatorKeyDistirbution[sha1EndPoint] = sha1KeyCount;


                //Tiger Hash
                TigerHash th = new TigerHash();
                byte[] tigerData = th.ComputeHash(Encoding.Unicode.GetBytes(key));


                IPEndPoint tigerEndPoint = FindNodeDefault(Convert.ToBase64String(tigerData, Base64FormattingOptions.None));

                KeyCount tigerHashKeyCount = _defaultLocatorKeyDistirbution[tigerEndPoint];
                tigerHashKeyCount.TigerHash = tigerHashKeyCount.TigerHash + 1;
                _defaultLocatorKeyDistirbution[tigerEndPoint] = tigerHashKeyCount;
            }

            Debug.WriteLine("Finished 10000 items test");

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("***Default Node Locator Tests****");

            sb.AppendLine(GetDefaultKeyResultString(_defaultLocatorKeyDistirbution));
            sb.AppendLine("*******************");
            sb.AppendLine(GetBase64ResultString(_defaultLocatorKeyDistirbution));
            sb.AppendLine("*******************");
            sb.AppendLine(GetSha1ResultString(_defaultLocatorKeyDistirbution));
            sb.AppendLine("*******************");
            sb.AppendLine(GetTigerHasResultString(_defaultLocatorKeyDistirbution));
            
            txtResults.Clear();

            txtResults.Text = sb.ToString();

            Debug.WriteLine("DONE");
        }

        private void DefaultNodeLocatorDefaultHasingIndex()
        {
            var keys = new uint[_nodes.Count * DefaultServerAddressMutations];

            int nodeIdx = 0;

            foreach (IPEndPoint node in _nodes)
            {
                var tmpKeys = GenerateDefaultKeyRanges(node, DefaultServerAddressMutations);

                for (var i = 0; i < tmpKeys.Length; i++)
                {
                    this._defaultLocatorServers[tmpKeys[i]] = node;
                }

                tmpKeys.CopyTo(keys, nodeIdx);
                nodeIdx += DefaultServerAddressMutations;
            }

            Array.Sort<uint>(keys);
            Interlocked.Exchange(ref this._defaulNodekeys, keys);
        }

        private static uint[] GenerateDefaultKeyRanges(IPEndPoint endPoint, int numberOfKeys)
        {
            const int KeyLength = 4;
            const int PartCount = 1; // (ModifiedFNV.HashSize / 8) / KeyLength; // HashSize is in bits, uint is 4 byte long

            var k = new uint[PartCount * numberOfKeys];

            // every server is registered numberOfKeys times
            // using UInt32s generated from the different parts of the hash
            // i.e. hash is 64 bit:
            // 00 00 aa bb 00 00 cc dd
            // server will be stored with keys 0x0000aabb & 0x0000ccdd
            // (or a bit differently based on the little/big indianness of the host)
            string address = endPoint.ToString();
            var fnv = new FNV1a();

            for (int i = 0; i < numberOfKeys; i++)
            {
                byte[] data = fnv.ComputeHash(Encoding.ASCII.GetBytes(String.Concat(address, "-", i)));

                for (int h = 0; h < PartCount; h++)
                {
                    k[i * PartCount + h] = BitConverter.ToUInt32(data, h * KeyLength);
                }
            }

            return k;
        }

        private IPEndPoint FindNodeDefault(string key)
        {
            if (this._defaulNodekeys.Length == 0) return null;

            uint itemKeyHash = BitConverter.ToUInt32(new FNV1a().ComputeHash(Encoding.UTF8.GetBytes(key)), 0);
            // get the index of the server assigned to this hash
            int foundIndex = Array.BinarySearch<uint>(this._defaulNodekeys, itemKeyHash);

            // no exact match
            if (foundIndex < 0)
            {
                // this is the nearest server in the list
                foundIndex = ~foundIndex;

                if (foundIndex == 0)
                {
                    // it's smaller than everything, so use the last server (with the highest key)
                    foundIndex = this._defaulNodekeys.Length - 1;
                }
                else if (foundIndex >= this._defaulNodekeys.Length)
                {
                    // the key was larger than all server keys, so return the first server
                    foundIndex = 0;
                }
            }

            if (foundIndex < 0 || foundIndex > this._defaulNodekeys.Length)
                return null;

            return this._defaultLocatorServers[this._defaulNodekeys[foundIndex]];
        }

        private string CreateKey(Guid rawProductId)
        {
            string rpId = rawProductId.ToString().ToLower();


            return string.Format("{0}|{1}", rpId, dcpId);
        }

        private string GetDefaultKeyResultString(Dictionary<IPEndPoint, KeyCount> keyDistribution )
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Default Key");
            foreach (IPEndPoint ipEndPoint in _defaultLocatorKeyDistirbution.Keys)
            {
                
                KeyCount counts = _defaultLocatorKeyDistirbution[ipEndPoint];

                sb.AppendLine(string.Format("IpAddress:{0} \tDefault : {1}",ipEndPoint.Address, counts.Default));
                Debug.WriteLine(string.Format("IpAddress:{0} \tDefault : {1}", ipEndPoint.Address, counts.Default));

                
            }

            return sb.ToString();
        }

        private string GetBase64ResultString(Dictionary<IPEndPoint, KeyCount> keyDistribution)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Base64 Key");
            foreach (IPEndPoint ipEndPoint in _defaultLocatorKeyDistirbution.Keys)
            {

                KeyCount counts = _defaultLocatorKeyDistirbution[ipEndPoint];

                sb.AppendLine(string.Format("IpAddress:{0} \tBase64 : {1}", ipEndPoint.Address, counts.Base64));
                Debug.WriteLine(string.Format("IpAddress:{0} \tBase64 : {1}", ipEndPoint.Address, counts.Base64));
            }

            return sb.ToString();
        }

        private string GetSha1ResultString(Dictionary<IPEndPoint, KeyCount> keyDistribution)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SHA1 Key");
            foreach (IPEndPoint ipEndPoint in _defaultLocatorKeyDistirbution.Keys)
            {
                KeyCount counts = _defaultLocatorKeyDistirbution[ipEndPoint];

                sb.AppendLine(string.Format("IpAddress:{0} \tSha1 : {1}", ipEndPoint.Address, counts.Sha1));
                Debug.WriteLine(string.Format("IpAddress:{0} \tSha1 : {1}", ipEndPoint.Address, counts.Sha1));
            }

            return sb.ToString();
        }


        private string GetTigerHasResultString(Dictionary<IPEndPoint, KeyCount> keyDistribution)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Tiger Key");
            foreach (IPEndPoint ipEndPoint in _defaultLocatorKeyDistirbution.Keys)
            {
                KeyCount counts = _defaultLocatorKeyDistirbution[ipEndPoint];

                sb.AppendLine(string.Format("IpAddress:{0} \tTiger : {1}", ipEndPoint.Address, counts.TigerHash));
                Debug.WriteLine(string.Format("IpAddress:{0} \tTiger : {1}", ipEndPoint.Address, counts.TigerHash));
            }

            return sb.ToString();
        }
    }

    public class KeyCount
    {
        public int Default { get; set; }
        public int Base64 { get; set; }
        public int Sha1 { get; set; }
        public int TigerHash { get; set; }
    }
}
