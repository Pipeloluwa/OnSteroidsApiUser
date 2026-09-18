import re

with open('OnSteroidsApiUser.Application/Services/RequestService.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = re.sub(r'\s*EncryptionChannel = req\.Encryption\?\.ChannelName,', '', content)

with open('OnSteroidsApiUser.Application/Services/RequestService.cs', 'w', encoding='utf-8') as f:
    f.write(content)
