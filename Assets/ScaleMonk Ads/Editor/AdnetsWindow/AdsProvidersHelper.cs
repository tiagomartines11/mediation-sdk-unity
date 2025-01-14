//  AdsProvidersHelper.cs
//
//  © 2020 ScaleMonk, Inc. All Rights Reserved.
// Licensed under the ScaleMonk SDK License Agreement
// https://www.scalemonk.com/legal/en-US/mediation-license-agreement/index.html 
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace ScaleMonk.Ads
{
    public class AdsProvidersHelper
    {
        const string iosAdsVersion = "1.1.0";
        const string androidAdsVersion = "1.0.23";

        public static string GetAdnetsXmlPath()
        {
            return Path.Combine(Application.dataPath, "../Packages/adnets.xml");
        }

        public static string GetAdnetsXmlSchemaPath()
        {
            return Path.Combine(GetLibPath(), "Editor/AdnetsSchema.xml");
        }

        public static string GetDependenciesPath()
        {
            return Path.Combine(GetLibPath(), "Editor/ScaleMonkAdsDependencies.xml");
        }

        static ScaleMonkXml ReadScaleMonkFromPath(ScaleMonkXml scaleMonkXmlBase, string path, bool local = false)
        {
            XmlElement xmlScalemonkNode = null;
            XmlNodeList xmlAdnetsNodeList = null;

            if (local)
            {
                var localDoc = new XmlDocument();
                localDoc.Load(GetAdnetsXmlPath());
                xmlScalemonkNode = localDoc.DocumentElement;
                var adnetsRoot = xmlScalemonkNode.SelectSingleNode("adnets");
                xmlAdnetsNodeList = xmlScalemonkNode != null ? adnetsRoot.SelectNodes("adnet") : null;
            }

            var adnetsDict = new Dictionary<string, AdnetXml>();
            foreach (var adnetBase in scaleMonkXmlBase.adnets)
            {
                adnetsDict[adnetBase.id] = adnetBase;
            }

            var doc = new XmlDocument();
            doc.Load(path);

            var root = doc.DocumentElement;

            if (root == null)
            {
                return new ScaleMonkXml();
            }

            // read scalemonk attributes to retrieve application ids
            string iOSApplicationID = xmlScalemonkNode != null ? (xmlScalemonkNode.Attributes["ios"].Value ?? "") : "";
            string androidApplicationID =
                xmlScalemonkNode != null ? (xmlScalemonkNode.Attributes["android"].Value ?? "") : "";

            // read adnets childs
            var nodes = root.SelectSingleNode("adnets").SelectNodes("adnet");
            foreach (XmlNode node in nodes)
            {
                var id = node.Attributes["id"].Value;
                var name = node.Attributes["name"].Value;
                var ios = bool.Parse(node.Attributes["ios"] != null
                    ? node.Attributes["ios"].Value ?? "false"
                    : "false");
                var android = bool.Parse(node.Attributes["android"] != null
                    ? node.Attributes["android"].Value ?? "false"
                    : "false");

                var configs = node.SelectNodes("adnetConfig");
                var adnetConfigs = new List<AdnetConfigXml>();
                foreach (XmlNode config in configs)
                {
                    var configConfig = config.Attributes["config"].Value;
                    var configPlatform = config.Attributes["platform"].Value;
                    var configName = config.Attributes["name"].Value;
                    var configValue = config.Attributes["value"] != null
                        ? config.Attributes["value"].Value ?? string.Empty
                        : string.Empty;
                    adnetConfigs.Add(new AdnetConfigXml(configConfig, configPlatform, configName, configValue));
                }

                AdnetXml currentAdnet = new AdnetXml(id, name, ios, android);
                var localNode = xmlAdnetsNodeList != null
                    ? xmlAdnetsNodeList.Cast<XmlNode>().FirstOrDefault(n =>
                        (n.Attributes["id"] != null ? n.Attributes["id"].Value : null) == id)
                    : null;

                if (localNode != null)
                {
                    currentAdnet.android = bool.Parse(localNode.Attributes["android"] != null
                        ? localNode.Attributes["android"].Value ?? "false"
                        : "false");
                    currentAdnet.ios = bool.Parse(localNode.Attributes["ios"] != null
                        ? localNode.Attributes["ios"].Value ?? "false"
                        : "false");
                    currentAdnet.iosVersion = localNode.Attributes["iosVersion"] != null
                        ? localNode.Attributes["iosVersion"].Value
                        : string.Empty;
                    currentAdnet.androidVersion = localNode.Attributes["androidVersion"] != null
                        ? localNode.Attributes["androidVersion"].Value
                        : string.Empty;
                }

                var newConfigs = new List<AdnetConfigXml>();
                if (localNode != null)
                {
                    var savedConfigs = localNode.SelectNodes("adnetConfig");
                    foreach (var newConfig in adnetConfigs)
                    {
                        var curConfig = savedConfigs.Cast<XmlNode>().FirstOrDefault(c =>
                            (c.Attributes["config"] != null ? c.Attributes["config"].Value : null) ==
                            newConfig.config &&
                            (c.Attributes["platform"] != null ? c.Attributes["platform"].Value : null) ==
                            newConfig.platform);
                        if (curConfig != null)
                        {
                            newConfig.value = curConfig.Attributes["value"].Value;
                        }

                        newConfigs.Add(newConfig);
                    }
                }
                else
                {
                    foreach (var newConfig in adnetConfigs)
                    {
                        newConfigs.Add(newConfig);
                    }  
                }

                currentAdnet.configs = newConfigs;
                adnetsDict[id] = currentAdnet;
            }

            ScaleMonkXml scaleMonkXml = new ScaleMonkXml();
            scaleMonkXml.adnets = adnetsDict.Values.ToList();
            scaleMonkXml.ios = iOSApplicationID;
            scaleMonkXml.android = androidApplicationID;
            return scaleMonkXml;
        }

        public static ScaleMonkXml ReadAdnetsConfigs()
        {
            var schemaPath = GetAdnetsXmlSchemaPath();
            var localPath = GetAdnetsXmlPath();
            var scaleMonkXml = ReadScaleMonkFromPath(new ScaleMonkXml(), schemaPath, File.Exists(localPath));
            return scaleMonkXml;
        }

        public static void SaveConfig(ScaleMonkXml scaleMonkXml)
        {
            if (scaleMonkXml.adnets == null) return;

            var doc = new XmlDocument();
            var xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            var root = doc.DocumentElement;

            doc.InsertBefore(xmlDeclaration, root);

            var scaleMonkElement = doc.CreateElement("scalemonk");
            scaleMonkElement.SetAttribute("ios", scaleMonkXml.ios);
            scaleMonkElement.SetAttribute("android", scaleMonkXml.android);
            var adnetsElement = doc.CreateElement("adnets");
            foreach (var adnet in scaleMonkXml.adnets)
            {
                var adnetElement = doc.CreateElement("adnet");
                adnetElement.SetAttribute("id", adnet.id);
                adnetElement.SetAttribute("name", adnet.name);
                adnetElement.SetAttribute("ios", adnet.ios.ToString());
                adnetElement.SetAttribute("iosVersion", adnet.iosVersion);
                adnetElement.SetAttribute("androidVersion", adnet.androidVersion);
                adnetElement.SetAttribute("android", adnet.android.ToString());

                if (adnet.configs != null && adnet.configs.Count > 0)
                {
                    foreach (var config in adnet.configs)
                    {
                        var configElement = doc.CreateElement("adnetConfig");
                        if (
                            ((adnet.ios && config.platform == "ios") || (adnet.android && config.platform == "android"))
                            && string.IsNullOrEmpty(config.value))
                        {
                            Debug.LogErrorFormat("Adnet {0} missing config {1} ({2})", adnet.name, config.name,
                                config.config);
                            return;
                        }

                        configElement.SetAttribute("config", config.config);
                        configElement.SetAttribute("platform", config.platform);
                        configElement.SetAttribute("name", config.name);
                        configElement.SetAttribute("value", config.value);
                        adnetElement.AppendChild(configElement);
                    }
                }

                adnetsElement.AppendChild(adnetElement);
            }

            scaleMonkElement.AppendChild(adnetsElement);
            doc.AppendChild(scaleMonkElement);

            var path = GetAdnetsXmlPath();
            Debug.Log("Saving config to " + path);
            doc.Save(path);

            UpdateNativeDependencies(scaleMonkXml);
        }

        static void UpdateNativeDependencies(ScaleMonkXml scaleMonkXml)
        {
            List<AdnetXml> adnets = scaleMonkXml.adnets;
            var doc = new XmlDocument();
            var xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            var root = doc.DocumentElement;

            doc.InsertBefore(xmlDeclaration, root);
            var dependenciesElement = doc.CreateElement("dependencies");

            if (!string.IsNullOrEmpty(scaleMonkXml.android))
            {
                UpdateAndroidDependencies(adnets, doc, dependenciesElement);
            }

            // TODO(lsebrie): this should be enabled when iOS doesn't receive app id on initialization anymore
            // if (!string.IsNullOrEmpty(scaleMonkXml.ios))
            // {
                UpdateIOSDependencies(adnets, doc, dependenciesElement);
            // }

            doc.AppendChild(dependenciesElement);

            var path = GetDependenciesPath();

            Debug.Log("Saving Android and Ios packages config to " + path);

            // Make file available to write
            if (File.Exists(path))
            {
                var pathFileAttributes = File.GetAttributes(path);
                File.SetAttributes(path, FileAttributes.Normal);
                doc.Save(path);
                File.SetAttributes(path, pathFileAttributes);
            }
            else
            {
                // save file
                doc.Save(path);
            }
        }

        static void UpdateIOSDependencies(List<AdnetXml> adnets, XmlDocument doc, XmlElement dependenciesElement)
        {
            var iosPodsElement = doc.CreateElement("iosPods");

            var adsPod = doc.CreateElement("iosPod");
            adsPod.SetAttribute("name", "ScaleMonkAds");
            adsPod.SetAttribute("version", iosAdsVersion);

            adsPod.AppendChild(CreateSourcesElement(doc));

            iosPodsElement.AppendChild(adsPod);

            foreach (var adnet in adnets)
            {
                if (!adnet.ios)
                {
                    continue;
                }

                var adnetPod = doc.CreateElement("iosPod");
                adnetPod.SetAttribute("name", string.Format("ScaleMonkAds-{0}", adnet.id));
                if (!string.IsNullOrEmpty(adnet.iosVersion))
                {
                    adnetPod.SetAttribute("version", adnet.iosVersion);
                }

                adnetPod.AppendChild(CreateSourcesElement(doc));

                // TODO: set configs to info.plist

                iosPodsElement.AppendChild(adnetPod);
            }

            dependenciesElement.AppendChild(iosPodsElement);
        }

        static void UpdateAndroidDependencies(List<AdnetXml> adnets, XmlDocument doc, XmlElement dependenciesElement)
        {
            var reposURL = new Dictionary<string, List<string>>
            {
                { "adcolony", new List<string> { "https://adcolony.bintray.com/AdColony" }},
                { "chartboost", new List<string> { "https://chartboostmobile.bintray.com/Chartboost" }},
                { "fyber", new List<string> { "https://fyber.bintray.com/marketplace" }},
                { "mintegral", new List<string>
                {
                    "https://dl-maven-android.mintegral.com/repository/mbridge_android_sdk_oversea",
                    "https://dl.bintray.com/mintegral-official/MBridge_AndroidSDK_Oversea",
                    "https://dl.bintray.com/mintegral-official/mintegral_ad_sdk_android_for_oversea",
                    "https://dl.bintray.com/mintegral-official/Mintegral_ad_SDK_Android"
                }},
                { "smaato", new List<string> { "https://s3.amazonaws.com/smaato-sdk-releases/" }},
            };

            var repositories = doc.CreateElement("repositories");
            var jfrogRepo = doc.CreateElement("repository");
            jfrogRepo.InnerText = "https://scalemonk.jfrog.io/artifactory/scalemonk-gradle-prod";
            repositories.AppendChild(jfrogRepo);

            var androidPackagesElement = doc.CreateElement("androidPackages");
            androidPackagesElement.AppendChild(getAndroidPackageForLib(doc, "ads", androidAdsVersion));
            foreach (var adnet in adnets)
            {
                if (adnet.android)
                {
                    androidPackagesElement.AppendChild(getAndroidPackageForLib(doc,
                        string.Format("ads-{0}", adnet.id.ToLower().Replace("-", "")),
                        adnet.androidVersion));

                    if (reposURL.ContainsKey(adnet.id.ToLower()))
                    {
                        reposURL[adnet.id.ToLower()].ForEach(url =>
                        {
                            var repo = doc.CreateElement("repository");
                            repo.InnerText = url;
                            repositories.AppendChild(repo);
                        });
                    }
                }
            }

            androidPackagesElement.AppendChild(repositories);

            // var gmsPackage = doc.CreateElement("androidPackage");
            // gmsPackage.SetAttribute("spec", "com.google.android.gms:play-services-base:15.0.1");
            // androidPackagesElement.AppendChild(gmsPackage);

            dependenciesElement.AppendChild(androidPackagesElement);
        }

        static XmlElement getAndroidPackageForLib(XmlDocument doc, string lib, string androidVersion)
        {
            var adsPackage = doc.CreateElement("androidPackage");
            adsPackage.SetAttribute("spec", string.Format("{0}:{1}:{2}", "com.scalemonk.libs", lib,
                string.IsNullOrEmpty(androidVersion) ? "+" : androidVersion));
            return adsPackage;
        }

        static XmlNode CreateSourcesElement(XmlDocument doc)
        {
            var sourcesElement = doc.CreateElement("sources");
            var iosPodspecSource = doc.CreateElement("source");

            iosPodspecSource.InnerText = "https://github.com/scalemonk/ios-podspecs-framework";

            sourcesElement.AppendChild(iosPodspecSource);

            return sourcesElement;
        }

        static string GetLibPath()
        {
            var localPath = Path.Combine(Application.dataPath, "ScaleMonk Ads/");
            var installedPath = Path.Combine(Application.dataPath, "ScaleMonk Ads/");

            if (Directory.Exists(installedPath))
            {
                return installedPath;
            }

            return localPath;
        }
    }
}