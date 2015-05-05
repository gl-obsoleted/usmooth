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

sys.path.append('./external/docopt')
from docopt import docopt

LICENSE_FILE = "LICENSE"
LICENSE_PREFIX = "!lic_info"

header_tmpl = string.Template( \
"""/*$license_prefix\n
$license_content
*/\n
""")

def iterate_source_files(directory, extension):
    for root, dirs, files in os.walk(directory):
        for name in files:
            if name.lower().endswith(extension):
                yield os.path.join(root, name)

def main(args):
    head = ""
    with open(LICENSE_FILE) as f:
        head = header_tmpl.substitute(license_prefix = LICENSE_PREFIX, license_content = f.read())

    for src in iterate_source_files(args['--src_dir'], args['--src_ext']):
        body = ""
        with open(src) as f:
            body = f.read()

        if body.startswith('/*'+LICENSE_PREFIX):
            continue

        with open(src, "w") as f:
            f.write(head + body)

        print("processed {}".format(src))

if __name__ == '__main__':
    args = docopt(__doc__)
    main(args)
