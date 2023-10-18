from PIL import Image
import os

name_path_imgns = 'Imagenes examen cerámicos'
path_imgns = os.listdir(name_path_imgns)

name_new_path = "Imagenes_Sanitarios"
new_path = os.mkdir(name_new_path)

for directorio in path_imgns:
    os.mkdir(name_new_path + '/' + directorio)
    for archivo in os.listdir(name_path_imgns + '/' + directorio):
        if archivo.endswith('.bmp'):
            img = Image.open(name_path_imgns + '/' + directorio + '/' + archivo)
            name = archivo.replace("bmp", "jpg")
            img.save(name_new_path + '/' + directorio + '/'+ name, quality=100)
    