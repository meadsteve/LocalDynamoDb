﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Amazon.DynamoDBv2;
using Amazon.Runtime;

namespace LocalDynamoDb
{
    public class LocalDynamo
    {
        private int _port;
        private Process Dynamo { get; set; }
        public AmazonDynamoDBClient Client { get; private set; }


        public LocalDynamo(int portNumber = 8000)
        {
            _port = portNumber;
            Dynamo = Create(_port);
            Client = CreateClient();
        }

        private Process Create(int portNumber)
        {
            var processJar = new Process();
            var arguments = String.Format("-Djava.library.path=./DynamoDBLocal_lib -jar DynamoDBLocal.jar -sharedDb -inMemory -port {0}", portNumber);

            processJar.StartInfo.FileName = "\"" + @"java" + "\"";
            processJar.StartInfo.Arguments = arguments;

            var rootFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var relativePath = "\\dynamodblocal";
            string absolutePath = Path.GetFullPath(rootFolder + relativePath);
            var jarFilePath = absolutePath + "\\DynamoDBLocal.jar";

            Console.WriteLine("Jar file path - " + jarFilePath);

            if (!File.Exists(jarFilePath))
            {
                throw new FileNotFoundException(
                    "DynamoDBLocal.jar not found in " + absolutePath +
                    ". Please review the README.txt for setup instructions.",
                    jarFilePath);
            }

            processJar.StartInfo.WorkingDirectory = absolutePath;
            processJar.StartInfo.UseShellExecute = false;
            processJar.StartInfo.RedirectStandardOutput = true;
            processJar.StartInfo.RedirectStandardError = true;
            return processJar;
        }

        public void Start()
        {
            Console.WriteLine("Starting in memory DynamoDb");
            var success = Dynamo.Start();
            Client = CreateClient();
            if (!success)
            {
                throw new Win32Exception("Error starting dynamo: " + Dynamo.StandardError.ReadToEnd());
            }
        }

        public void Stop()
        {
            Console.WriteLine("Stopping in memory DynamoDb");
            try
            {
                Dynamo.Kill();
            }
            catch (Win32Exception)
            {
                Console.WriteLine(Dynamo.StandardError.ReadToEnd());
                throw;
            }
        }

        private AmazonDynamoDBClient CreateClient()
        {
            var config = new AmazonDynamoDBConfig { ServiceURL = String.Format("http://localhost:{0}", _port) };
            var credentials = new BasicAWSCredentials("A NIGHTINGALE HAS NO NEED FOR KEYS", "IT OPENS DOORS WITH ITS SONG");
            return new AmazonDynamoDBClient(credentials, config);
        }
    }
}