@echo off
curl -X POST -H "Content-Type: application/vnd.schemaregistry.v1+json" ^
  --data "{\"schema\": \"{\\\"type\\\": \\\"record\\\", \\\"name\\\": \\\"TestRecord\\\", \\\"namespace\\\": \\\"com.example\\\", \\\"fields\\\": [{\\\"name\\\": \\\"StringField\\\", \\\"type\\\": \\\"string\\\"}, {\\\"name\\\": \\\"IntField\\\", \\\"type\\\": \\\"int\\\"}, {\\\"name\\\": \\\"BooleanField\\\", \\\"type\\\": \\\"boolean\\\"}, {\\\"name\\\": \\\"DoubleField\\\", \\\"type\\\": \\\"double\\\"}, {\\\"name\\\": \\\"ArrayField\\\", \\\"type\\\": {\\\"type\\\": \\\"array\\\", \\\"items\\\": \\\"string\\\"}}, {\\\"name\\\": \\\"EnumField\\\", \\\"type\\\": {\\\"type\\\": \\\"enum\\\", \\\"name\\\": \\\"TestEnum\\\", \\\"symbols\\\": [\\\"ONE\\\", \\\"TWO\\\", \\\"THREE\\\"]}}]}\"}" ^
  http://localhost:8081/subjects/test-schema-value/versions