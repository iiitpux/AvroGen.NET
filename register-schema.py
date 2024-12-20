import json
import requests

# Read the schema file
with open('schemas/test-schema.avsc', 'r') as f:
    schema = json.load(f)

# Prepare the payload
payload = {
    'schema': json.dumps(schema)
}

# Register the schema
response = requests.post(
    'http://localhost:8081/subjects/test-schema-value/versions',
    headers={'Content-Type': 'application/vnd.schemaregistry.v1+json'},
    json=payload
)

print(response.status_code)
print(response.text)
