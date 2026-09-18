import re

with open('OnSteroidsApiUser.Domain/Models/Request/RequestModels.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = re.sub(r'\s*public string\? EncryptionChannel \{ get; set; \}', '', content)

with open('OnSteroidsApiUser.Domain/Models/Request/RequestModels.cs', 'w', encoding='utf-8') as f:
    f.write(content)
