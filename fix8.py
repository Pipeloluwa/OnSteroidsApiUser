import re

with open('OnSteroidsApiUser.Infrastructure/Repositories/RequestRepository.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = re.sub(r'\s*p\.Add\(\"@EncryptionChannel\", req\.EncryptionChannel\);', '', content)

with open('OnSteroidsApiUser.Infrastructure/Repositories/RequestRepository.cs', 'w', encoding='utf-8') as f:
    f.write(content)
