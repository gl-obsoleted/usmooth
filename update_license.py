""" update license information for source files

Usage:
	update_license.py	--src_dir=<dir> --src_ext=<ext>

Options:
	-d --src_dir=<dir>			source directory
	-e --src_ext=<ext>			source extension

"""
import os
import sys
import string 
import codecs

sys.path.append(os.path.join(os.path.dirname(__file__), 'external', 'docopt'))
from docopt import docopt

LICENSE_FILE = "LICENSE"
LICENSE_PREFIX = "!lic_info"

header_tmpl = string.Template("""/*$license_prefix\n$license_content\n*/\n""")

def iterate_source_files(directory, extension):
    for root, dirs, files in os.walk(directory):
        for name in files:
            if name.lower().endswith(extension):
                yield os.path.join(root, name)

# http://stackoverflow.com/questions/13590749/reading-unicode-file-data-with-bom-chars-in-python
def detect_by_bom(path,default=None):
    with open(path, 'rb') as f:
        raw = f.read(4)    #will read less if the file is smaller
    for enc,boms in \
            ('utf-8-sig',(codecs.BOM_UTF8,)),\
            ('utf-16',(codecs.BOM_UTF16_LE,codecs.BOM_UTF16_BE)),\
            ('utf-32',(codecs.BOM_UTF32_LE,codecs.BOM_UTF32_BE)):
        if any(raw.startswith(bom) for bom in boms): return enc
    return default

def main(args):
    head = ""
    with open(LICENSE_FILE) as f:
        head = header_tmpl.substitute(license_prefix = LICENSE_PREFIX, license_content = f.read())

    for src in iterate_source_files(args['--src_dir'], args['--src_ext']):
    	src_encoding = detect_by_bom(src)

        body = ""
        with open(src) as f:
            body = f.read()
            if src_encoding:
            	body = body.decode(src_encoding)

        if body.startswith('/*'+LICENSE_PREFIX):
            continue

        # print(src)        # for debugging
        # print(body[:20])

        with open(src, "w") as f:
            if src_encoding:
            	f.write((head + body).encode(src_encoding))
            else:
            	f.write(head + body)

        print("processed {}".format(src))

if __name__ == '__main__':
    args = docopt(__doc__)
    main(args)
