"""Gera listas YOLO disjuntas sem mover imagens ou anotações originais."""
import hashlib
import json
from pathlib import Path
import random


def split_dataset(dataset, seed=42):
    dataset = Path(dataset)
    images = sorted(
        path for path in (dataset / "train/images").iterdir()
        if path.suffix.lower() in {".jpg", ".jpeg", ".png", ".bmp", ".webp"}
    )
    groups = []
    # Mantém variantes Roboflow e arquivos idênticos no mesmo conjunto.
    for image in images:
        label = dataset / "train/labels" / f"{image.stem}.txt"
        if not label.is_file():
            raise ValueError(f"Anotação ausente: {label.name}")
        keys = {image.stem.split(".rf.")[0], hashlib.sha256(image.read_bytes()).hexdigest()}
        matching = [group for group in groups if group[0] & keys]
        members = [image]
        for group in matching:
            keys |= group[0]
            members.extend(group[1])
            groups.remove(group)
        groups.append((keys, members))
    if len(groups) < 3:
        raise ValueError("São necessários ao menos três grupos independentes de imagens.")
    random.Random(seed).shuffle(groups)
    n_val = max(1, round(len(groups) * 0.15))
    n_test = max(1, round(len(groups) * 0.15))
    partitions = {
        "val": groups[:n_val], "test": groups[n_val:n_val + n_test],
        "train": groups[n_val + n_test:],
    }
    counts = {}
    for name, selected in partitions.items():
        files = sorted(image for _, members in selected for image in members)
        # O YOLO resolve ./ relativamente ao arquivo de lista.
        paths = ["./" + image.relative_to(dataset).as_posix() for image in files]
        (dataset / f"{name}.txt").write_text("\n".join(paths) + "\n", encoding="utf-8")
        counts[name] = len(files)
    metadata = {"seed": seed, "images": len(images), "groups": len(groups), "counts": counts}
    (dataset / "split_metadata.json").write_text(json.dumps(metadata, indent=2) + "\n", encoding="utf-8")
    return metadata


if __name__ == "__main__":
    print(json.dumps(split_dataset(Path(__file__).resolve().parent / "dataset"), indent=2))
