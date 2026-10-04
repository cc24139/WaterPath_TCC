"""Treina um classificador de presença por alvo, preservando a imagem inteira.

Requer ultralytics, torch, torchvision e Pillow no ambiente de treinamento.
Não é importado pela API de detecção existente.
"""
import argparse
import inspect
from pathlib import Path

from PIL import ImageOps
import torchvision.transforms as T
from ultralytics import YOLO
from ultralytics.data.dataset import ClassificationDataset
from ultralytics.models.yolo.classify import ClassificationTrainer, ClassificationValidator

BASE = Path(__file__).resolve().parent
CLASSES = ('lixo', 'drainage_connection', 'urbano')

class FullFrameResize:
    def __init__(self, size):
        self.size = size

    def __call__(self, image):
        return ImageOps.pad(ImageOps.exif_transpose(image).convert('RGB'), (self.size, self.size), color=(114, 114, 114))

def dataset(root, args, augment, prefix, names):
    options = dict(root=root, args=args, augment=augment, prefix=prefix)
    if 'names' in inspect.signature(ClassificationDataset.__init__).parameters:
        options['names'] = names
    data = ClassificationDataset(**options)
    transforms = [FullFrameResize(args.imgsz)]
    if augment:
        transforms.append(T.RandomHorizontalFlip(p=args.fliplr))
    # Sem crops, apagamento de objetos ou alteração artificial de cor da água.
    transforms.append(T.ToTensor())
    data.torch_transforms = T.Compose(transforms)
    return data

class FullFrameValidator(ClassificationValidator):
    def build_dataset(self, img_path):
        return dataset(img_path, self.args, False, self.args.split, self.names)

class FullFrameTrainer(ClassificationTrainer):
    def build_dataset(self, img_path, mode='train', batch=None):
        return dataset(img_path, self.args, mode == 'train', mode, self.data['names'])

def run(args):
    targets = CLASSES if args.target == 'all' else (args.target,)
    for target in targets:
        data = (args.dataset / target).resolve()
        if not (data/'train/presente').is_dir():
            raise ValueError(f'Dataset de classificação ausente: {data}')
        model = YOLO(args.model)
        model.train(
            data=str(data), trainer=FullFrameTrainer, epochs=args.epochs,
            imgsz=args.imgsz, batch=args.batch, device=args.device,
            seed=42, workers=0, fliplr=0.5, flipud=0.0,
            project=str(BASE/'runs/classify_presence'), name=target,
        )
        # Durante o treino o val usa build_dataset do treinador; aqui garantimos
        # a mesma transformação na validação independente do checkpoint final.
        trained = YOLO(str(Path(model.trainer.save_dir)/'weights/best.pt'))
        trained.val(data=str(data), validator=FullFrameValidator, split='val', imgsz=args.imgsz, workers=0)
        if args.evaluate_test:
            trained.val(data=str(data), validator=FullFrameValidator, split='test', imgsz=args.imgsz, workers=0)

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--target', choices=(*CLASSES, 'all'), default='all')
    parser.add_argument('--dataset', type=Path, default=BASE/'dataset_multilabel/yolo_classification_v2')
    parser.add_argument('--model', default='yolov8n-cls.pt')
    parser.add_argument('--epochs', type=int, default=50)
    parser.add_argument('--imgsz', type=int, default=320)
    parser.add_argument('--batch', type=int, default=16)
    parser.add_argument('--device', default='cpu')
    parser.add_argument('--evaluate-test', action='store_true', help='Use apenas na avaliação final, depois de fixar parâmetros.')
    run(parser.parse_args())
