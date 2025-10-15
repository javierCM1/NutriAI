# train_lora.py
import torch
from transformers import AutoModelForCausalLM, AutoTokenizer
from datasets import load_dataset
from peft import LoraConfig, get_peft_model, prepare_model_for_kbit_training, PeftModel
from transformers import TrainingArguments, Trainer

# === 0️⃣ Detectar GPU / CPU y mostrar info ===
if torch.cuda.is_available():
    device = torch.device("cuda")
    print(f"✅ GPU detectada: {torch.cuda.get_device_name(0)}")
else:
    device = torch.device("cpu")
    print("⚠️ No se detectó GPU. Se usará CPU, el entrenamiento será más lento.")

# === 1️⃣ Cargar modelo base de Hugging Face ===
base = AutoModelForCausalLM.from_pretrained(
    "meta-llama/Llama-3.2-1B",
    device_map="auto",  # PyTorch asigna automáticamente a GPU si está disponible
    torch_dtype=torch.float16 if device.type=="cuda" else torch.float32
)
tokenizer = AutoTokenizer.from_pretrained("meta-llama/Llama-3.2-1B")
tokenizer.pad_token = tokenizer.eos_token

# === 2️⃣ Cargar dataset ===
dataset = load_dataset("json", data_files="nutri_train.jsonl")["train"]

# === 3️⃣ Configurar LoRA ===
config = LoraConfig(
    r=16,
    lora_alpha=32,
    target_modules=["q_proj", "v_proj"],
    lora_dropout=0.05,
    bias="none",
    task_type="CAUSAL_LM",
)
model = get_peft_model(base, config)

# === 4️⃣ Tokenizar dataset ===
def tokenize(sample):
    text = f"Prompt:\n{sample['prompt']}\n\nRespuesta:\n{sample['response']}"
    tokenized = tokenizer(
        text,
        truncation=True,
        max_length=512,
        padding="max_length"
    )
    tokenized["labels"] = tokenized["input_ids"].copy()
    return tokenized

dataset = dataset.map(tokenize, batched=False)

# === 5️⃣ Entrenamiento ===
args = TrainingArguments(
    output_dir="./lora-output",
    per_device_train_batch_size=1,
    num_train_epochs=3,
    learning_rate=2e-4,
    fp16=True if device.type=="cuda" else False,  # FP16 solo en GPU
    logging_steps=10,
    save_total_limit=1,
    save_strategy="epoch",
)

trainer = Trainer(
    model=model,
    args=args,
    train_dataset=dataset,
)

trainer.train()

# === 6️⃣ Guardar adaptador LoRA ===
model.save_pretrained("./lora-output")
tokenizer.save_pretrained("./lora-output")
print("✅ Adaptador LoRA guardado en ./lora-output")

# === 7️⃣ Fusionar LoRA con modelo base ===
model = PeftModel.from_pretrained(base, "./lora-output")
model = model.merge_and_unload()
model.save_pretrained("./final-llama3-lora")
tokenizer.save_pretrained("./final-llama3-lora")
print("✅ LoRA fusionado con modelo base. Modelo final listo en ./final-llama3-lora")
