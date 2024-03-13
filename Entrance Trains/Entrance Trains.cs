using GTA;
using System;
using GTA.Math;
using GTA.Native;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EntranceTrains
{
    public class EntranceTrains : Script
    {
        ScriptSettings config;

        Vehicle main_train;
        Ped main_driver;
        Blip damn_train;

        Random rnd = new Random();
        public bool train_created = false;
        float acceleration_start = 0.1f;
        float acceleration = 0.01f;
        float braking = 0.01f;
        float emergency_break = 0.1f;
        float inertia_stop = 0.001f;
        int help_displayed = 0;
        int help_time = 7000;
        int timera = 0;
        int current_brain_command = -1;
        int focus_time = 0;
        int dead_inside_flag = 0;
        float max_distance;
        int config_blip;

        private Vector3[] coords = new Vector3[]
        {
        new Vector3(1084.48f, 3231.45f, 39.2565f),
        new Vector3(-348.166f, 3777.43f, 70.5792f),
        new Vector3(-521.651f, 5144.55f, 89.5354f),
        new Vector3(-117.049f, 6165.34f, 30.5851f),
        new Vector3(1714.72f, 6327.65f, 44.4615f),
        new Vector3(2407.8f, 5870.89f, 59.5511f),
        new Vector3(3024.44f, 4619.21f, 61.7123f),
        new Vector3(2811.73f, 3256.31f, 49.7621f),
        new Vector3(2565.54f, 2188.55f, 31.2609f),
        new Vector3(2633.9f, 953.431f, 69.3521f),
        new Vector3(2515.82f, -213.83f, 92.2707f),
        new Vector3(1542.56f, -958.438f, 67.1674f),
        new Vector3(669.273f, -1032.81f, 21.2806f),
        new Vector3(475.495f, -2617.64f, 11.5055f),
        new Vector3(558.392f, -1393.45f, 20.6264f),
        new Vector3(1287.52f, -858.74f, 48.4408f),
        new Vector3(2271.04f, -516.791f, 94.686f),
        new Vector3(2670.63f, 570.374f, 93.3828f),
        new Vector3(2039.33f, 1642.88f, 73.9948f),
        new Vector3(2217.92f, 2629.95f, 49.2856f),
        new Vector3(2895.82f, 3551.96f, 44.1713f),
        new Vector3(1372.39f, 3303.23f, 38.1892f),

        };

        public EntranceTrains()
        {
            Tick += OnTick;
            Aborted += OnAborted;

            config = ScriptSettings.Load("Scripts\\Entrance Trains.ini");
            config_blip = config.GetValue<int>("MAIN", "blips", 1);
        }

        void OnTick(object sender, EventArgs e)
        {

            DeleteVanillaTrains();

            if (!train_created)
            {
                if (damn_train != null && damn_train.Exists())
                {
                    damn_train.Delete();
                    damn_train = null;
                }

                int nearestIndex = FindNearestIndex(Game.Player.Character.Position, coords);
                main_train = CreateTrain(nearestIndex);
                main_driver = main_train.CreatePedOnSeat(VehicleSeat.Driver, PedHash.Lsmetro01SMM);

                if (config_blip == 1)
                {
                    damn_train = main_train.AddBlip();
                    damn_train.Sprite = BlipSprite.Train;
                    damn_train.Name = "Damn Train";
                    damn_train.IsShortRange = true;
                }


                train_created = true;

                max_distance = Game.Player.Character.Position.DistanceTo(main_train.Position) + 500.0f;
            }

            SetBrainChoice(main_driver, main_train);
            HijackTrain(main_driver);
            if (main_train != null && main_train.Exists() && Function.Call<bool>(Hash.IS_PED_IN_VEHICLE, Game.Player.Character, main_train, false))
            {
                TrainPlayerControls(main_train);
                ShowHelpControl(main_train);
            }

            if (train_created && main_train != null && main_train.Exists() && Game.Player.Character.Position.DistanceTo(main_train.Position) > max_distance)
            {
                DeleteModTrain();
                train_created = false;
                int nearestIndex = FindNearestIndex(Game.Player.Character.Position, coords);
                main_train = CreateTrain(nearestIndex);
                main_driver = main_train.CreatePedOnSeat(VehicleSeat.Driver, PedHash.Lsmetro01SMM);

                if (config_blip == 1)
                {
                    damn_train = main_train.AddBlip();
                    damn_train.Sprite = BlipSprite.Train;
                    damn_train.Name = "Damn Train";
                    damn_train.IsShortRange = true;
                }

                train_created = true;
            }
        }

        private int FindNearestIndex(Vector3 playerPosition, Vector3[] coordinates)
        {
            int nearestIndex = -1;
            float minDistance = float.MaxValue;

            for (int i = 0; i < coordinates.Length; i++)
            {
                float distance = Vector3.Distance(playerPosition, coordinates[i]);
                if (distance >= 300.0f && distance < minDistance)
                {
                    minDistance = distance;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }

        void LoadTrainModels()
        {
            List<string> models = new List<string>() {
               "freight2",
               "freightcont1",
               "freightcar",
               "freightcar2",
               "freightcont2",
               "tankercar",
               "freightgrain",
            };

            foreach (string model in models)
            {
                var veh_model = new Model(Function.Call<VehicleHash>(Hash.GET_HASH_KEY, model));
                veh_model.Request(500);
                while (!veh_model.IsLoaded) Script.Wait(100);
            }
        }

        Vehicle CreateTrain(int nearestIndex)
        {
            Vehicle train;
            Random rand = new Random();

            LoadTrainModels();
            var ped_model = new Model(Function.Call<PedHash>(Hash.GET_HASH_KEY, "s_m_m_lsmetro_01"));
            ped_model.Request(500);
            while (!ped_model.IsLoaded) Script.Wait(100);

            int type = rand.Next(27);
            train = null;

            bool direction = (nearestIndex != 2) ? true : false;

            train = Function.Call<Vehicle>(Hash.CREATE_MISSION_TRAIN, type, coords[nearestIndex].X, coords[nearestIndex].Y, coords[nearestIndex].Z, direction, false, false);

            while (train == null) Script.Wait(100);

            Function.Call(Hash.SET_TRAIN_CRUISE_SPEED, train, 15.0f);

            return train;
        }

        private bool IsRiskOfCollision(Ped player, Vehicle train)
        {
            float visible = 60f;
            int time = Function.Call<int>(Hash.GET_CLOCK_HOURS);
            Weather weather = World.Weather;

            visible -= (time >= 0 && time <= 6) ? 20 : 0;

            switch (weather)
            {
                case Weather.Raining:
                case Weather.Smog:
                case Weather.Foggy:
                    visible -= 20f;
                    break;
                case Weather.Clear:
                case Weather.Clearing:
                case Weather.Clouds:
                case Weather.ExtraSunny:
                    visible += 10f;
                    break;
            }

            Vector3 playerPosition = player.Position;
            Vector3 trainPosition = train.Position;
            Vector3 trainForwardVector = train.ForwardVector;

            Vector3 targetPosition = trainPosition + trainForwardVector * 50.0f;

            Vector3 deviationVector = new Vector3(-trainForwardVector.Y, trainForwardVector.X, trainForwardVector.Z).Normalized;

            targetPosition += deviationVector * 10.0f;

            RaycastResult result = World.Raycast(targetPosition, playerPosition, IntersectFlags.Everything, train);

            if (result.DidHit && result.HitEntity != train)
            {
                if (result.HitEntity is Ped || result.HitEntity is Vehicle)
                {
                    float angle = Vector3.Angle(trainForwardVector, (result.HitEntity.Position - trainPosition).Normalized);
                    if (angle < 3.0f && result.HitEntity.Position.DistanceTo(train.Position) < visible)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        void ShowHelpControl(Vehicle train)
        {
            if (train_created && Function.Call<bool>(Hash.IS_PED_IN_VEHICLE, Game.Player.Character, train, false))
            {
                if (dead_inside_flag == 1)
                {
                    dead_inside_flag = 0;
                }

                if (help_displayed == 0)
                {
                    timera = Game.GameTime + help_time;
                    help_displayed = 1;
                }

                if (timera > Game.GameTime)
                {
                    GTA.UI.Screen.ShowHelpTextThisFrame("Hold ~INPUT_VEH_ACCELERATE~ to accelerate.~n~Hold ~INPUT_VEH_BRAKE~ to brake.~n~Hold ~INPUT_VEH_PASSENGER_AIM~ to aim then use ~INPUT_VEH_PASSENGER_ATTACK~ to shoot.");
                }
                else
                {
                    help_displayed = -1;
                }
            }
        }

        void HijackTrain(Ped driver)
        {
            if (driver != null && driver.Exists())
            {
                if (driver.IsDead)
                {
                    driver.Delete();
                    dead_inside_flag = 1;
                }

                if (!driver.IsDead && Game.Player.Character.Position.DistanceTo(driver.Position) < 10f)
                {
                    GTA.UI.Screen.ShowHelpTextThisFrame("Kill the driver to steal the train.");
                }
            }
        }

        void SetBrainChoice(Ped driver, Vehicle train)
        {
            if (train_created && driver != null && driver.Exists() && !driver.IsDead)
            {
                if (IsRiskOfCollision(Game.Player.Character, train))
                {
                    SetCommandDriveNPC(train, driver, 2); //emergency brake
                    current_brain_command = 0; //danger
                }
                else
                {
                    if (!IsRiskOfCollision(Game.Player.Character, train) && current_brain_command == 0)
                    {
                        SetCommandDriveNPC(train, driver, 2); //brake
                        current_brain_command = 1; //focus
                        focus_time = Game.GameTime + 10000;
                    }
                    else
                    {
                        if (current_brain_command == 1 && Game.GameTime > focus_time)
                        {
                            current_brain_command = -1; //normal
                        }
                        else
                        {
                            if (current_brain_command == -1 && train.Speed < 10.0)
                            {
                                SetCommandDriveNPC(train, driver, 0); //acceleration
                            }
                        }
                    }
                }
            }
            else
            {
                if (train_created && dead_inside_flag == 1 && train.Speed > 0.0)
                {
                    SetCommandDriveDeadNPC(train);
                }
            }
        }

        void SetCommandDriveDeadNPC(Vehicle train)
        {
            float current_speed = train.Speed;
            current_speed -= braking;

            if (current_speed < 0)
            {
                current_speed = 0.0f;
            }

            Function.Call(Hash.SET_TRAIN_SPEED, train, current_speed);
            Function.Call(Hash.SET_TRAIN_CRUISE_SPEED, train, current_speed);
        }

        void SetCommandDriveNPC(Vehicle train, Ped driver, int pedal) //acceleration (0), brake (1), emergency brake (2), inertia stop (4)
        {
            float current_speed = train.Speed;

            switch (pedal)
            {
                case 0:
                    current_speed += acceleration;
                    break;

                case 1:
                    current_speed -= braking;
                    break;

                case 2:
                    current_speed -= emergency_break;
                    break;
            }

            if (current_speed < 0)
            {
                current_speed = 0.0f;
            }

            Function.Call(Hash.SET_TRAIN_SPEED, train, current_speed);
            Function.Call(Hash.SET_TRAIN_CRUISE_SPEED, train, current_speed);
        }

        void TrainPlayerControls(Vehicle train)
        {
            if (Function.Call<bool>(Hash.IS_PED_IN_VEHICLE, Game.Player.Character, train, false))
            {
                float speed = train.Speed;
                if (Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 0, 71) && speed < 10.0)
                {
                    speed += acceleration_start;
                }
                else
                {
                    if (Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 0, 71) && (speed >= 10.0 && speed < 60.0))
                    {
                        speed += acceleration;
                    }
                    else
                    {
                        if (Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 0, 72) && speed > 0.0)
                        {
                            speed -= emergency_break;
                        }
                    }
                }

                if (0.0 > speed)
                {
                    speed = 0.0f;
                }

                Function.Call(Hash.SET_TRAIN_SPEED, train, speed);
                Function.Call(Hash.SET_TRAIN_CRUISE_SPEED, train, speed);
            }
        }

        void DeleteVanillaTrains()
        {
            for (int i = 0; i <= 3; i++)
            {
                Function.Call(Hash.SWITCH_TRAIN_TRACK, i, false);
            }
        }

        void DeleteModTrain()
        {
            if (main_train != null && main_train.Exists())
            {
                foreach (Vehicle veh in World.GetAllVehicles())
                {
                    if (Function.Call<bool>(Hash.IS_MISSION_TRAIN, veh))
                    {
                        veh.Delete();
                    }
                }
            }

            if (main_driver != null && main_driver.Exists())
            {
                main_driver.Delete();
            }
        }

        void OnAborted(object sender, EventArgs e)
        {
            if (main_train != null && main_train.Exists())
            {
                DeleteModTrain();
            }

            if (main_driver != null && main_driver.Exists())
            {
                main_driver.Delete();
            }

            if (damn_train != null && damn_train.Exists())
            {
                damn_train.Delete();
            }
        }
    }
}