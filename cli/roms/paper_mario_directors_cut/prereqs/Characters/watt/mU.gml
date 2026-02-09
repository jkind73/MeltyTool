#define mUWatt
if(animated_sprite_check_run())
{
        if(image_index > 4 && image_index < 6 && !stepSound)
        {
            	sound_play(sndFootsteps);
            	stepSound = true;
		d3d_instance_create(x+lengthdir_x(5,direction+180),y+lengthdir_y(5,direction+180),z,objSmoke);
        }
	else if(image_index > 0 && image_index < 1)
            	stepSound = false;
}

flyZ = 0;


var n;
n = .0125;
wattRay1 += n;
wattRay2 += n;

wattRay1 = wattRay1 mod 1;
wattRay2 = wattRay2 mod 1;