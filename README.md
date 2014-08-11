Particular.OpsBI
================

Reporting and BI on the Particular Platform using Elasticsearch and Kibana

![Demo Dashboard](https://s3.amazonaws.com/uploads.hipchat.com/84513/614935/LAT6mA6SpFOApHK/OpsBI.Messages.PNG)

## How to run

0. Have Elasticsearch running
1. Download from http://elasticsearch.org/download
2. Go edit config\elasticsearch.yml and edit:
	* `cluster.name` to something non-default. Your GitHub username will do.
	* replica and shard count, by specifing `index.number_of_shards: 1` and `index.number_of_replicas: 0` (unless you know what you are doing)
3. Make sure you have JAVA_HOME properly set up
4. Run `elasticsearch\bin\elasticsearch.bat`
5. Compile and run KibanaHost and OpsBI.Dashboards
6. Fix `ServiceControlUrl` in Ops.Importer's Program.cs (it needs to be a static, and to point at a valid ServiceControl endpoint).
7. Compile and run OpsBI.Importer
8. Go to http://localhost:3579/ and load the Ops.BI Kibana dashboards (folder icon at the upper right corner)
